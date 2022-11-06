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

namespace WebApp.Pages.CheckersStates
{
    public class EditModel : PageModel
    {
        private readonly DAL.Db.AppDbContext _context;

        public EditModel(DAL.Db.AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public CheckersState CheckersState { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || _context.CheckersStates == null)
            {
                return NotFound();
            }

            var checkersstate =  await _context.CheckersStates.FirstOrDefaultAsync(m => m.Id == id);
            if (checkersstate == null)
            {
                return NotFound();
            }
            CheckersState = checkersstate;
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

            _context.Attach(CheckersState).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CheckersStateExists(CheckersState.Id))
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

        private bool CheckersStateExists(int id)
        {
          return (_context.CheckersStates?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
