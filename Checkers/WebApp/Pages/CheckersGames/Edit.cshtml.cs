using DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Domain;

namespace WebApp.Pages.CheckersGames
{
    public class EditModel : PageModel
    {
        private readonly IRepositoryContext _ctx;

        public EditModel(IRepositoryContext ctx)
        {
            _ctx = ctx;
        }

        [BindProperty] public CheckersGame CheckersGame { get; set; } = default!;

        public IActionResult OnGet(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var checkersGame = _ctx.CheckersGameRepository.GetById(id.Value);

            if (checkersGame == null)
            {
                return NotFound();
            }

            CheckersGame = checkersGame;

            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _ctx.CheckersGameRepository.RefreshPartial(CheckersGame);
            _ctx.CheckersGameRepository.Update(CheckersGame);

            return RedirectToPage("./Index");
        }

        private bool CheckersGameExists(int id)
        {
            return _ctx.CheckersGameRepository.Exists(id);
        }
    }
}