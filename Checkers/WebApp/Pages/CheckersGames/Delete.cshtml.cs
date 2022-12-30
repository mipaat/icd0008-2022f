using DAL;
using Domain;
using WebApp.MyLibraries.PageModels;

namespace WebApp.Pages.CheckersGames
{
    public class DeleteModel : DeleteModel<CheckersGame>
    {
        protected override IRepository<CheckersGame> Repository => Ctx.CheckersGameRepository;

        public DeleteModel(IRepositoryContext ctx) : base(ctx)
        {
        }
    }
}