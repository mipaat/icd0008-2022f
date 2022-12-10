using DAL;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Domain;

namespace WebApp.Pages.CheckersGames
{
    public class IndexModel : PageModel
    {
        private readonly IRepositoryContext _ctx;

        public IndexModel(IRepositoryContext ctx)
        {
            _ctx = ctx;
        }

        public IList<CheckersGame> CheckersGame { get; set; } = default!;

        public Task OnGet()
        {
            CheckersGame = _ctx.CheckersGameRepository.GetAll().ToList();
            return Task.CompletedTask;
        }
    }
}