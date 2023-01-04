using DAL;
using Domain.Model;
using Microsoft.AspNetCore.Mvc;
using WebApp.MyLibraries.PageModels;

namespace WebApp.Pages.CheckersStates;

public class IndexModel : IndexModel<CheckersState>
{
    [BindProperty(SupportsGet = true)] public int? CheckersGameId { get; set; }
    [BindProperty(SupportsGet = true)] public CheckersGame? CheckersGame { get; set; }

    public IndexModel(IRepositoryContext ctx) : base(ctx)
    {
    }

    protected override IRepository<CheckersState> Repository => Ctx.CheckersStateRepository;

    public override IActionResult OnGet()
    {
        if (CheckersGameId == null) return base.OnGet();
        var checkersGame = Ctx.CheckersGameRepository.GetById(CheckersGameId.Value, true);
        if (checkersGame == null) return NotFound();
        CheckersGame = checkersGame;
        Entities = CheckersGame.CheckersStates!.ToList();
        return Page();
    }
}