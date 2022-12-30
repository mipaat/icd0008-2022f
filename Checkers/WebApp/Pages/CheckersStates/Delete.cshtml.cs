using DAL;
using Domain;
using WebApp.MyLibraries.PageModels;

namespace WebApp.Pages.CheckersStates
{
    public class DeleteModel : DeleteModel<CheckersState>
    {
        protected override IRepository<CheckersState> Repository => Ctx.CheckersStateRepository;

        public DeleteModel(IRepositoryContext ctx) : base(ctx)
        {
        }
    }
}