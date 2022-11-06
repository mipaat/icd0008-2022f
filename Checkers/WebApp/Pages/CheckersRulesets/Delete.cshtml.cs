using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DAL.Db;
using Domain;

namespace WebApp.Pages.CheckersRulesets
{
    public class DeleteModel : PageModel
    {
        private readonly DAL.Db.AppDbContext _context;

        public DeleteModel(DAL.Db.AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
      public CheckersRuleset CheckersRuleset { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || _context.CheckersRulesets == null)
            {
                return NotFound();
            }

            var checkersruleset = await _context.CheckersRulesets.FirstOrDefaultAsync(m => m.Id == id);

            if (checkersruleset == null)
            {
                return NotFound();
            }
            else 
            {
                CheckersRuleset = checkersruleset;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null || _context.CheckersRulesets == null)
            {
                return NotFound();
            }
            var checkersruleset = await _context.CheckersRulesets.FindAsync(id);

            if (checkersruleset != null)
            {
                CheckersRuleset = checkersruleset;
                _context.CheckersRulesets.Remove(CheckersRuleset);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
