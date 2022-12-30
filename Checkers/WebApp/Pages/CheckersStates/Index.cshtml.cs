using DAL;
using Domain;
using WebApp.MyLibraries.PageModels;

namespace WebApp.Pages.CheckersStates
{
    public class IndexModel : IndexModel<CheckersState>
    {
        protected override IRepository<CheckersState> Repository => Ctx.CheckersStateRepository;

        public IndexModel(IRepositoryContext ctx) : base(ctx)
        {
        }
    }
}