using DAL;
using Domain;
using GameBrain;
using Microsoft.AspNetCore.Mvc;
using WebApp.MyLibraries.PageModels;

namespace WebApp.Pages.CheckersGames;

public class Ended : PageModelDb
{
    public CheckersBrain Brain { get; set; } = default!;

    [BindProperty(SupportsGet = true)] public int? PlayerId { get; set; }
    
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

    public Ended(IRepositoryContext ctx) : base(ctx)
    {
    }
}