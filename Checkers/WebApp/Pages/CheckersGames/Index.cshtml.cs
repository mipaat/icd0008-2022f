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
    [BindProperty(SupportsGet = true)] public string? RulesetNameQuery { get; set; }
    [BindProperty(SupportsGet = true)] public string? PlayerNameQuery { get; set; }

    [BindProperty(SupportsGet = true)] public CompletionFilter CompletionFilter { get; set; } = default!;

    protected override IRepository<CheckersGame> Repository => Ctx.CheckersGameRepository;

    private static bool StringContains(string? query, string? compare)
    {
        var normalizedQuery = query?.ToLower().Trim() ?? "";
        var normalizedCompare = compare?.ToLower().Trim() ?? "";
        return normalizedCompare.Contains(normalizedQuery);
    }

    public IActionResult OnGet()
    {
        var result = Repository.GetAll(
            CompletionFilter.FilterFunc,
            AiTypeFilter.FilterFunc).AsEnumerable();

        if (PlayerNameQuery != null)
            result = result.Where(cg => StringContains(PlayerNameQuery, cg.WhitePlayerName)
                                        || StringContains(PlayerNameQuery, cg.BlackPlayerName));

        if (RulesetNameQuery != null)
            result = result.Where(cg => StringContains(RulesetNameQuery, cg.CheckersRuleset?.TitleText));

        Entities = result.ToList();

        return Page();
    }
}