using DAL;
using Domain;
using GameBrain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.CheckersGames;

public class Ended : PageModel
{
    private readonly IRepositoryContext _ctx;

    public Ended(IRepositoryContext ctx)
    {
        _ctx = ctx;
    }
    
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
        var checkersGame = _ctx.CheckersGameRepository.GetById(id);
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