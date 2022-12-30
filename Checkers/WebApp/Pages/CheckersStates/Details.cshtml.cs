using DAL;
using Domain;
using WebApp.MyLibraries.PageModels;

namespace WebApp.Pages.CheckersStates
{
    public class DetailsModel : EntityModel<CheckersState>
    {
        public DetailsModel(IRepositoryContext ctx) : base(ctx)
        {
        }

        protected override IRepository<CheckersState> Repository => Ctx.CheckersStateRepository;
    }
}
