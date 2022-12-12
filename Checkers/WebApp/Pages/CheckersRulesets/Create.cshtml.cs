using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using DAL.Db;
using Domain;

namespace WebApp.Pages.CheckersRulesets
{
    public class CreateModel : PageModel
    {
        private readonly IRepositoryContext _ctx;

        public CreateModel(IRepositoryContext ctx)
        {
            _ctx = ctx;
        }

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

            _ctx.CheckersRulesetRepository.Add(CheckersRuleset);

            return Task.FromResult<IActionResult>(RedirectToPage("./Index"));
        }
    }
}