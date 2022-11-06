using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DAL.Db;
using Domain;

namespace WebApp.Pages.CheckersRulesets
{
    public class EditModel : PageModel
    {
        private readonly DAL.Db.AppDbContext _context;

        public EditModel(DAL.Db.AppDbContext context)
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

            var checkersruleset =  await _context.CheckersRulesets.FirstOrDefaultAsync(m => m.Id == id);
            if (checkersruleset == null)
            {
                return NotFound();
            }
            CheckersRuleset = checkersruleset;
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(CheckersRuleset).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CheckersRulesetExists(CheckersRuleset.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool CheckersRulesetExists(int id)
        {
          return (_context.CheckersRulesets?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
