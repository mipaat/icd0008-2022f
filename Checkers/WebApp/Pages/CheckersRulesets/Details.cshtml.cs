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
    public class DetailsModel : PageModel
    {
        private readonly DAL.Db.AppDbContext _context;

        public DetailsModel(DAL.Db.AppDbContext context)
        {
            _context = context;
        }

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
    }
}
