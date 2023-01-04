using Common;
using DAL;
using Domain.Model;
using Microsoft.AspNetCore.Mvc;
using WebApp.MyLibraries.PageModels;

namespace WebApp.Pages.CheckersRulesets;

public class Select : IndexModel<CheckersRuleset>
{
    [BindProperty(SupportsGet = true)] public string Query { get; set; } = "";

    public Select(IRepositoryContext ctx) : base(ctx)
    {
    }

    protected override ICheckersRulesetRepository Repository => Ctx.CheckersRulesetRepository;

    [BindProperty(SupportsGet = true)] public int? Id { get; set; }
    [BindProperty(SupportsGet = true)] public bool Edit { get; set; }

    public override IActionResult OnGet()
    {
        Entities = Repository.GetAllSaved().Where(cr => Utils.StringAnyContain(Query, cr.ToString())).ToList();
        if (Id == null) return Page();
        var checkersRuleset = Repository.GetById(Id.Value);
        if (checkersRuleset == null) return NotFound();
        if (Edit != true)
        {
            return RedirectToPage("/CheckersGames/Create",
                new { selectedCheckersRulesetId = Id });
        }

        return RedirectToPage("./Edit", new { Id, RedirectToCheckersGame = true, createCopy = true });
    }
}