using DAL;
using Domain.Model.Helpers;
using GameBrain;
using Microsoft.AspNetCore.Mvc;
using WebApp.MyLibraries.PageModels;

namespace WebApp.Pages.CheckersGames;

public class Ended : PageModelDb
{
    public Ended(IRepositoryContext ctx) : base(ctx)
    {
    }

    public CheckersBrain Brain { get; set; } = default!;

    [BindProperty(SupportsGet = true)] public int? PlayerId { get; set; }

    public EPlayerColor PlayerColor
    {
        get
        {
            if (PlayerId == null) return Brain.CurrentTurnPlayerColor;
            try
            {
                return Player.GetPlayerColor(PlayerId.Value);
            }
            catch (ArgumentOutOfRangeException)
            {
                return Brain.CurrentTurnPlayerColor;
            }
        }
    }

    public IActionResult OnGet(int id)
    {
        var checkersGame = Ctx.CheckersGameRepository.GetById(id);
        if (checkersGame == null)
            return RedirectToPage("/Index", new { error = $"No CheckersGame with ID {id} found!" });

        Brain = new CheckersBrain(checkersGame);
        if (!Brain.Ended)
            return RedirectToPage("/Index",
                new
                {
                    error = $"Can't show end screen for CheckersGame that hasn't ended (ID {checkersGame.Id})!"
                });

        return Page();
    }
}