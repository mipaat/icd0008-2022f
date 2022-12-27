using DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Domain;

namespace WebApp.Pages.CheckersGames
{
    public class DeleteModel : PageModel
    {
        private readonly IRepositoryContext _ctx;

        public DeleteModel(IRepositoryContext ctx)
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

        public IActionResult OnPost(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var checkersGame = _ctx.CheckersGameRepository.Remove(id.Value);

            if (checkersGame == null) return NotFound();

            return RedirectToPage("./Index");
        }
    }
}