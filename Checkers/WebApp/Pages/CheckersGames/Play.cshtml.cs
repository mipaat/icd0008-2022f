using DAL;
using Domain;
using GameBrain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.CheckersGames;

public class Play : PageModel
{
    private readonly IRepositoryContext _ctx;

    public Play(IRepositoryContext ctx)
    {
        _ctx = ctx;
    }

    public CheckersBrain Brain { get; set; } = default!;
    [BindProperty(SupportsGet = true)] public int? PlayerId { get; set; }
    public int GameId { get; set; }

    public EPlayerColor PlayerColor
    {
        get
        {
            return PlayerId switch
            {
                0 => EPlayerColor.Black,
                1 => EPlayerColor.White,
                _ => Brain.CurrentTurnPlayerColor
            };
        }
    }

    public string? PlayerName
    {
        get
        {
            return PlayerId switch
            {
                0 => Brain.PlayerName(PlayerColor),
                1 => Brain.PlayerName(PlayerColor),
                _ => null
            };
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

    public bool IsPieceMovable(int x, int y) // TODO: Check if piece actually has available moves
    {
        return !Brain.IsAiTurn && Brain.IsPieceMovable(PlayerColor, x, y);
    }

    public bool IsMovableTo(int x, int y)
    {
        return !Brain.IsAiTurn && FromSet && Brain.IsMoveValid(PlayerColor, FromX!.Value, FromY!.Value, x, y);
    }

    public IActionResult OnGet(int? id, bool endTurn = false, bool aiMoveAllowed = false)
    {
        if (id == null)
        {
            return RedirectToPage("/Index", new { error = "NULL is not a valid game ID!" });
        }

        CheckersGame? game;
        try
        {
            game = _ctx.CheckersGameRepository.GetById(id.Value);
        }
        catch (InsufficientCheckersStatesException)
        {
            return RedirectToPage("/Index", new { error = $"Game with ID {id.Value} appears to be corrupted!" });
        }

        if (game == null)
        {
            return RedirectToPage("/Index", new { error = $"No game with ID {id.Value} found!" });
        }

        if (game.CheckersRuleset == null || game.CurrentCheckersState == null)
        {
            return RedirectToPage("/Index", new { error = $"Failed to load game with ID {id.Value}!" });
        }

        game.CurrentCheckersState.DeserializeGamePieces();

        GameId = id.Value;
        Brain = new CheckersBrain(game);

        if (Brain.Ended)
        {
            return RedirectToPage("./Ended", new { Id = GameId, PlayerId });
        }

        if (Brain.IsAiTurn)
        {
            if (!aiMoveAllowed) return Page();

            var aiColor = Brain.CurrentTurnPlayerColor;
            while (Brain.CurrentTurnPlayerColor == aiColor)
            {
                Brain.MoveAi();
            }

            _ctx.CheckersGameRepository.Upsert(Brain.GetSaveGameState());
            return Reset;
        }

        if (endTurn && Brain.EndTurnAllowed && PlayerColor ==
            Brain[Brain.LastMovedToX!.Value, Brain.LastMovedToY!.Value]?.Player)
        {
            Brain.EndTurn();
            _ctx.CheckersGameRepository.Upsert(Brain.GetSaveGameState());
            return Reset;
        }

        if (FromSet && !IsPieceMovable(FromX!.Value, FromY!.Value))
        {
            return Reset;
        }

        if (FromSet && ToSet)
        {
            if (IsMovableTo(ToX!.Value, ToY!.Value))
            {
                Brain.Move(FromX!.Value, FromY!.Value, ToX!.Value, ToY!.Value);
                _ctx.CheckersGameRepository.Upsert(Brain.GetSaveGameState());
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

        return Page();
    }
}