using DAL;
using Domain.Model;
using Microsoft.AspNetCore.Mvc;
using WebApp.MyLibraries.PageModels;

namespace WebApp.Pages.CheckersRulesets;

public class EditModel : EditModel<CheckersRuleset>
{
    [BindProperty(SupportsGet = true)] public bool RedirectToCheckersGame { get; set; }
    [BindProperty(SupportsGet = true)] public bool CreateCopy { get; set; }
    [BindProperty(SupportsGet = true)] public bool? Saved { get; set; }

    public EditModel(IRepositoryContext ctx) : base(ctx)
    {
    }

    protected override IRepository<CheckersRuleset> Repository => Ctx.CheckersRulesetRepository;

    public override IActionResult OnGet(int? id)
    {
        var result = base.OnGet(id);
        if (!Success) return result;
        if (!Entity.IsEditable && !CreateCopy)
            return RedirectToPage("/Index", new { error = $"Can't edit CheckersRuleset with ID {id}!" });
        if (CreateCopy) Entity = Entity.GetClone(Saved);
        return result;
    }

    public override IActionResult OnPost()
    {
        var result = base.OnPost();
        if (!Success || !RedirectToCheckersGame) return result;
        return RedirectToPage("/CheckersGames/Create",
            new { selectedCheckersRulesetId = Entity.Id });
    }
}