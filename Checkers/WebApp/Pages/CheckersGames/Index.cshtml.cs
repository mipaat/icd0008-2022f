using Common;
using DAL;
using DAL.Filters;
using Domain.Model;
using Microsoft.AspNetCore.Mvc;
using WebApp.MyLibraries.PageModels;

namespace WebApp.Pages.CheckersGames;

public class IndexModel : IndexModel<CheckersGame>
{
    public IndexModel(IRepositoryContext ctx) : base(ctx)
    {
    }

    [BindProperty(SupportsGet = true)] public AiTypeFilter AiTypeFilter { get; set; } = default!;
    [BindProperty(SupportsGet = true)] public string? RulesetQuery { get; set; }
    [BindProperty(SupportsGet = true)] public string? PlayerNameQuery { get; set; }

    [BindProperty(SupportsGet = true)] public CompletionFilter CompletionFilter { get; set; } = default!;

    protected override IRepository<CheckersGame> Repository => Ctx.CheckersGameRepository;

    protected override IEnumerable<FilterFunc<CheckersGame>> Filters => new List<FilterFunc<CheckersGame>>
    {
        CompletionFilter.FilterFunc,
        AiTypeFilter.FilterFunc,
        new(iq => iq.Where(
                cg => Utils.StringAnyContain(PlayerNameQuery, cg.WhitePlayerName, cg.BlackPlayerName)),
            false),
        new(iq => iq.Where(
                cg => Utils.StringAnyContain(RulesetQuery, cg.CheckersRuleset!.ToString())),
            false)
    };
}