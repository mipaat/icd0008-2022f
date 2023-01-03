using Common;
using DAL;
using DAL.Filters;
using Domain.Model;
using Microsoft.AspNetCore.Mvc;
using WebApp.MyLibraries.PageModels;

namespace WebApp.Pages.CheckersGames;

public class IndexModel : RepositoryModel<CheckersGame>
{
    public IndexModel(IRepositoryContext ctx) : base(ctx)
    {
    }

    public IList<CheckersGame> Entities { get; set; } = default!;

    [BindProperty(SupportsGet = true)] public AiTypeFilter AiTypeFilter { get; set; } = default!;
    [BindProperty(SupportsGet = true)] public string? RulesetQuery { get; set; }
    [BindProperty(SupportsGet = true)] public string? PlayerNameQuery { get; set; }

    [BindProperty(SupportsGet = true)] public CompletionFilter CompletionFilter { get; set; } = default!;

    protected override IRepository<CheckersGame> Repository => Ctx.CheckersGameRepository;

    public IActionResult OnGet()
    {
        var result = Repository.GetAll(
            CompletionFilter.FilterFunc,
            AiTypeFilter.FilterFunc).AsEnumerable();

        if (PlayerNameQuery != null)
            result = result.Where(cg =>
                Utils.StringAnyContain(PlayerNameQuery, cg.WhitePlayerName, cg.BlackPlayerName));

        if (RulesetQuery != null)
            result = result.Where(cg => Utils.StringAnyContain(RulesetQuery, cg.CheckersRuleset?.ToString()));

        Entities = result.ToList();

        return Page();
    }
}