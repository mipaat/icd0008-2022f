using DAL;
using Microsoft.AspNetCore.Mvc;
using Domain;
using WebApp.MyLibraries.PageModels;

namespace WebApp.Pages.CheckersRulesets
{
    public class CreateModel : PageModelDb
    {
        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public CheckersRuleset CheckersRuleset { get; set; } = default!;
        

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Task.FromResult<IActionResult>(Page());
            }

            Ctx.CheckersRulesetRepository.Add(CheckersRuleset);

            return Task.FromResult<IActionResult>(RedirectToPage("./Index"));
        }

        public CreateModel(IRepositoryContext ctx) : base(ctx)
        {
        }
    }
}