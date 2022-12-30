using DAL;
using Domain;
using WebApp.MyLibraries.PageModels;

namespace WebApp.Pages.CheckersGames
{
    public class IndexModel : IndexModel<CheckersGame>
    {
        public IndexModel(IRepositoryContext ctx) : base(ctx)
        {
        }

        protected override IRepository<CheckersGame> Repository => Ctx.CheckersGameRepository;
    }
}