using DAL;
using Domain.Model;
using Microsoft.AspNetCore.Mvc;
using WebApp.MyLibraries.PageModels;

namespace WebApp.Pages.CheckersRulesets;

public class IndexModel : IndexModel<CheckersRuleset>
{
    public IndexModel(IRepositoryContext ctx) : base(ctx)
    {
    }

    [BindProperty(SupportsGet = true)] public bool ShowNonSaved { get; set; }

    protected override IRepository<CheckersRuleset> Repository => Ctx.CheckersRulesetRepository;

    public override IActionResult OnGet()
    {
        Entities =
            (ShowNonSaved ? Ctx.CheckersRulesetRepository.GetAll() : Ctx.CheckersRulesetRepository.GetAllSaved())
            .ToList();
        return Page();
    }
}