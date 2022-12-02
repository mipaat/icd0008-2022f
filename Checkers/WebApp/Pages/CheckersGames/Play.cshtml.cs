using DAL;
using Domain;
using GameBrain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.CheckersGames;

public class Play : PageModel
{
    private readonly ICheckersGameRepository _games;

    public Play(ICheckersGameRepository games)
    {
        _games = games;
    }

    public CheckersBrain Brain { get; set; } = default!;
    public string PlayerId { get; set; } = default!;
    public int GameId { get; set; }

    public EPlayerColor PlayerColor => Brain.PlayerColor(PlayerId);

    public bool IsPlayerTurn => Brain.IsPlayerTurn(PlayerId);

    [BindProperty(SupportsGet = true)] public int? FromX { get; set; }
    [BindProperty(SupportsGet = true)] public int? FromY { get; set; }
    [BindProperty(SupportsGet = true)] public int? ToX { get; set; }
    [BindProperty(SupportsGet = true)] public int? ToY { get; set; }
    public bool FromSet => FromX != null && FromY != null;
    public bool ToSet => ToX != null && ToY != null;
    public GamePiece? PieceBeingMoved => FromSet ? Brain[FromX!.Value, FromY!.Value] : null;

    private RedirectToPageResult Reset => RedirectToPage("", new { id = GameId, playerId = PlayerId });

    public bool IsPieceMovable(int x, int y) // TODO: Check if piece actually has available moves
    {
        return Brain.IsPieceMovable(PlayerId, x, y);
    }

    public bool IsMovableTo(int x, int y)
    {
        return FromSet && Brain.IsMoveValid(PlayerId, FromX!.Value, FromY!.Value, x, y);
    }

    public async Task<IActionResult> OnGet(int? id, string? playerId)
    {
        if (id == null)
        {
            return RedirectToPage("/Index", new { error = $"NULL is not a valid game ID!" });
        }

        CheckersGame game;
        try
        {
            game = await Task.Run(() => _games.GetById(id.Value));
        }
        catch (InvalidOperationException)
        {
            return RedirectToPage("/Index", new { error = $"No game with ID {id.Value} found!" });
        }
        catch (InsufficientCheckersStatesException)
        {
            return RedirectToPage("/Index", new { error = $"Game with ID {id.Value} appears to be corrupted!" });
        }

        if (game.CheckersRuleset == null || game.CurrentCheckersState == null)
        {
            return RedirectToPage("/Index", new { error = $"Failed to load game with ID {id.Value}!" });
        }

        game.CurrentCheckersState.DeserializeGamePieces();

        if ((playerId != game.BlackPlayerId && playerId != game.WhitePlayerId) || playerId == null)
        {
            var playerIdText = playerId ?? "NULL";
            return RedirectToPage("/Index",
                new { error = $"Player with ID '{playerIdText}' not found in game with ID {id}!" });
        }

        GameId = id.Value;
        Brain = new CheckersBrain(game);
        PlayerId = playerId;

        if (FromSet && !IsPieceMovable(FromX!.Value, FromY!.Value))
        {
            return Reset;
        }

        if (FromSet && ToSet)
        {
            if (IsMovableTo(ToX!.Value, ToY!.Value))
            {
                Brain.Move(FromX!.Value, FromY!.Value, ToX!.Value, ToY!.Value);
                _games.Upsert(Brain.GetSaveGameState());
            }

            return Reset;
        }

        return Page();
    }
}