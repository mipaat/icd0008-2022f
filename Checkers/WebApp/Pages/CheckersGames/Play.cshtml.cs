using DAL;
using Domain;
using Domain.Model;
using Domain.Model.Helpers;
using GameBrain;
using Microsoft.AspNetCore.Mvc;
using WebApp.MyLibraries;
using WebApp.MyLibraries.PageModels;
using Timer = System.Timers.Timer;

namespace WebApp.Pages.CheckersGames;

public class Play : PageModelDb
{
    public Play(IRepositoryContext ctx) : base(ctx)
    {
    }

    public CheckersBrain Brain { get; set; } = default!;
    [BindProperty(SupportsGet = true)] public int? PlayerId { get; set; }
    public int GetPlayerId => PlayerId ?? Brain.CurrentTurnPlayer.Id;
    public int GameId { get; set; }

    public Player Player => Brain.Player(PlayerColor);

    public EPlayerColor PlayerColor
    {
        get
        {
            var defaultColor = (Brain.WhitePlayer.IsAi, Brain.BlackPlayer.IsAi) switch
            {
                (true, false) => Brain.BlackPlayer.Color,
                (false, true) => Brain.WhitePlayer.Color,
                _ => Brain.CurrentTurnPlayerColor
            };
            if (PlayerId == null) return defaultColor;
            try
            {
                return Player.GetPlayerColor(PlayerId.Value);
            }
            catch (ArgumentOutOfRangeException)
            {
                return defaultColor;
            }
        }
    }

    public bool IsPlayerTurn => Brain.IsPlayerTurn(PlayerColor);

    [BindProperty(SupportsGet = true)] public int? FromX { get; set; }
    [BindProperty(SupportsGet = true)] public int? FromY { get; set; }
    [BindProperty(SupportsGet = true)] public int? ToX { get; set; }
    [BindProperty(SupportsGet = true)] public int? ToY { get; set; }
    public bool FromSet => FromX != null && FromY != null;
    public bool ToSet => ToX != null && ToY != null;
    public GamePiece? PieceBeingMoved => FromSet ? Brain[FromX!.Value, FromY!.Value] : null;

    private RedirectToPageResult Reset =>
        RedirectToPage("", new { id = GameId, playerId = PlayerId });

    public bool DrawResolutionExpected => Brain.DrawResolutionExpectedFrom(PlayerColor);

    public bool IsPieceMovable(int x, int y)
    {
        return !Brain.IsAiTurn && Brain.IsPieceMovable(PlayerColor, x, y);
    }

    public bool IsMovableTo(int x, int y)
    {
        return !Brain.IsAiTurn && FromSet && Brain.IsMoveValid(PlayerColor, FromX!.Value, FromY!.Value, x, y);
    }

    private void SaveGame()
    {
        Ctx.CheckersGameRepository.Upsert(Brain.CheckersGame);
    }

    private CheckersGame GetCheckersGameById(int? id)
    {
        if (id == null) throw new PageException("NULL is not a valid game ID!");

        CheckersGame? game;
        try
        {
            game = Ctx.CheckersGameRepository.GetById(id.Value);
        }
        catch (InsufficientCheckersStatesException)
        {
            throw new PageException($"Game with ID {id.Value} appears to be corrupted!");
        }

        if (game == null) throw new PageException($"No game with ID {id.Value} found!");

        if (game.CheckersRuleset == null || game.CurrentCheckersState == null)
            throw new PageException($"Failed to load game with ID {id.Value}!");

        return game;
    }

    private void InitializeProperties(int? id)
    {
        var game = GetCheckersGameById(id);

        game.CurrentCheckersState!.DeserializeGamePieces();

        GameId = id!.Value;
        Brain = new CheckersBrain(game);
    }

    private IActionResult MakeAiMove()
    {
        var aiColor = Brain.CurrentTurnPlayerColor;
        var timer = new Timer(1000);
        var timerElapsed = false;
        timer.Elapsed += (_, _) => timerElapsed = true;
        timer.Start();
        while (Brain.CurrentTurnPlayerColor == aiColor) Brain.MoveAi();

        while (!timerElapsed)
        {
        }

        timer.Stop();

        SaveGame();
        return Reset;
    }

    private IActionResult MakeMove()
    {
        if (IsMovableTo(ToX!.Value, ToY!.Value))
        {
            Brain.Move(FromX!.Value, FromY!.Value, ToX!.Value, ToY!.Value);
            SaveGame();
        }

        return Brain.LastMoveState == EMoveState.CanContinue
            ? RedirectToPage("",
                new
                {
                    id = GameId, playerId = PlayerId, fromX = Brain.LastMovedToX,
                    FromY = Brain.LastMovedToY
                })
            : Reset;
    }

    public IActionResult OnGet(int? id, bool endTurn = false, bool aiMoveAllowed = false,
        bool forfeit = false, bool proposeDraw = false, bool? acceptDraw = null, bool forceDraw = false)
    {
        try
        {
            InitializeProperties(id);
        }
        catch (PageException e)
        {
            return e.RedirectTarget;
        }

        if (DrawResolutionExpected && acceptDraw == null && !Brain.IsAiTurn) return Page();

        if (forceDraw)
        {
            Brain.Draw();
            SaveGame();
            return Reset;
        }
        
        if (forfeit)
        {
            Brain.Forfeit(Player.Color);
            SaveGame();
            return Reset;
        }

        if (proposeDraw)
        {
            Brain.ProposeDraw(PlayerColor);
            SaveGame();
            return Reset;
        }

        if (acceptDraw != null && DrawResolutionExpected)
        {
            if (acceptDraw.Value)
                Brain.AcceptDraw(PlayerColor);
            else
                Brain.RejectDraw(PlayerColor);
            SaveGame();
            return Reset;
        }

        if (Brain.Ended) return RedirectToPage("./Ended", new { Id = GameId, PlayerId });

        if (Brain.IsAiTurn) return aiMoveAllowed ? MakeAiMove() : Page();

        if (endTurn && Brain.EndTurnAllowed && PlayerColor ==
            Brain[Brain.LastMovedToX!.Value, Brain.LastMovedToY!.Value]?.Player)
        {
            Brain.EndTurn();
            SaveGame();
            return Reset;
        }

        if (FromSet && !IsPieceMovable(FromX!.Value, FromY!.Value)) return Reset;

        if (FromSet && ToSet) return MakeMove();

        return Page();
    }
}