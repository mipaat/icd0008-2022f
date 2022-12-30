using DAL;
using Domain;
using GameBrain;
using Microsoft.AspNetCore.Mvc;
using WebApp.MyLibraries.PageModels;

namespace WebApp.Pages.CheckersGames;

public class Launch : PageModelDb
{
    public int Id { get; set; }
    public CheckersBrain CheckersBrain { get; set; } = default!;
    public CheckersGame CheckersGame { get; set; } = default!;

    public IActionResult OnGet(int id)
    {
        var checkersGame = Ctx.CheckersGameRepository.GetById(id);
        if (checkersGame == null) return NotFound();
        Id = id;
        CheckersGame = checkersGame;
        CheckersBrain = new CheckersBrain(checkersGame);
        return (CheckersGame.BlackAiType == null, CheckersGame.WhiteAiType == null) switch
        {
            (true, true) => Page(),
            (false, false) => RedirectToPage("./Play", new { id = CheckersGame.Id }),
            (false, true) => RedirectToPage("./Play", new { id = CheckersGame.Id, playerId = 1 }),
            (true, false) => RedirectToPage("./Play", new { id = CheckersGame.Id, playerId = 0 })
        };
    }

    public Launch(IRepositoryContext ctx) : base(ctx)
    {
    }
}