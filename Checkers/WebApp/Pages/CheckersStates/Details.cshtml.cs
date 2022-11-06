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
    public class DetailsModel : PageModel
    {
        private readonly DAL.Db.AppDbContext _context;

        public DetailsModel(DAL.Db.AppDbContext context)
        {
            _context = context;
        }

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
    }
}
