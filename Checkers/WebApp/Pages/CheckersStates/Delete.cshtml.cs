using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DAL.Db;
using Domain;

namespace WebApp.Pages.CheckersStates
{
    public class DeleteModel : PageModel
    {
        private readonly DAL.Db.AppDbContext _context;

        public DeleteModel(DAL.Db.AppDbContext context)
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

            var checkersstate = await _context.CheckersStates.FirstOrDefaultAsync(m => m.Id == id);

            if (checkersstate == null)
            {
                return NotFound();
            }
            else 
            {
                CheckersState = checkersstate;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null || _context.CheckersStates == null)
            {
                return NotFound();
            }
            var checkersstate = await _context.CheckersStates.FindAsync(id);

            if (checkersstate != null)
            {
                CheckersState = checkersstate;
                _context.CheckersStates.Remove(CheckersState);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
