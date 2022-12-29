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

        public IList<CheckersGame> CheckersGames { get; set; } = default!;

        public Task OnGet()
        {
            CheckersGames = _ctx.CheckersGameRepository.GetAll().ToList();
            return Task.CompletedTask;
        }
    }
}