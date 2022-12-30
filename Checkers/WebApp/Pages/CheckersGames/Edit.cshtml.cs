using DAL;
using Domain;
using WebApp.MyLibraries.PageModels;

namespace WebApp.Pages.CheckersGames
{
    public class EditModel : EditModel<CheckersGame>
    {
        protected override IRepository<CheckersGame> Repository => Ctx.CheckersGameRepository;

        public EditModel(IRepositoryContext ctx) : base(ctx)
        {
        }
    }
}