using Common;
using DAL;
using DAL.Filters;
using Domain.Model;
using Microsoft.AspNetCore.Mvc;
using WebApp.MyLibraries.PageModels;

namespace WebApp.Pages.CheckersRulesets;

public class IndexModel : IndexModel<CheckersRuleset>
{
    public IndexModel(IRepositoryContext ctx) : base(ctx)
    {
    }

    [BindProperty(SupportsGet = true)] public SavedFilter SavedFilter { get; set; } = default!;
    [BindProperty(SupportsGet = true)] public string? RulesetQuery { get; set; }

    protected override IRepository<CheckersRuleset> Repository => Ctx.CheckersRulesetRepository;

    protected override IEnumerable<FilterFunc<CheckersRuleset>> Filters => new List<FilterFunc<CheckersRuleset>>
    {
        SavedFilter.FilterFunc,
        new(iq => iq
                .Where(cr => Utils.StringAnyContain(RulesetQuery, cr.ToString())),
            false),
    };
}