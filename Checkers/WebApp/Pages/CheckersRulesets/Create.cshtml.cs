using DAL;
using Domain.Model;
using Microsoft.AspNetCore.Mvc;
using WebApp.MyLibraries.PageModels;

namespace WebApp.Pages.CheckersRulesets;

public class CreateModel : PageModelDb
{
    public CreateModel(IRepositoryContext ctx) : base(ctx)
    {
    }

    [BindProperty] public CheckersRuleset CheckersRuleset { get; set; } = default!;

    public IActionResult OnGet()
    {
        return Page();
    }


    // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
    public Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid) return Task.FromResult<IActionResult>(Page());

        Ctx.CheckersRulesetRepository.Add(CheckersRuleset);

        return Task.FromResult<IActionResult>(RedirectToPage("./Index"));
    }
}