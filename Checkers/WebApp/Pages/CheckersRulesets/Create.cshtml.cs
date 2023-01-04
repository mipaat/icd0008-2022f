using DAL;
using Domain.Model;
using Microsoft.AspNetCore.Mvc;
using WebApp.MyLibraries.PageModels;

namespace WebApp.Pages.CheckersRulesets;

public class CreateModel : PageModelDb
{
    [BindProperty(SupportsGet = true)] public bool RedirectToCheckersGame { get; set; }

    public CreateModel(IRepositoryContext ctx) : base(ctx)
    {
    }

    [BindProperty] public CheckersRuleset CheckersRuleset { get; set; } = default!;
    [BindProperty(SupportsGet = true)] public bool AllowSaved { get; set; }

    public IActionResult OnGet()
    {
        return Page();
    }

    // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
    public IActionResult OnPost()
    {
        if (!ModelState.IsValid) return Page();

        Ctx.CheckersRulesetRepository.Add(CheckersRuleset);

        if (RedirectToCheckersGame)
            return RedirectToPage("/CheckersGames/Create", new { selectedCheckersRulesetId = CheckersRuleset.Id });
        return RedirectToPage("./Index");
    }
}