using DAL;
using Domain;
using WebApp.MyLibraries.PageModels;

namespace WebApp.Pages.CheckersStates
{
    public class EditModel : EditModel<CheckersState>
    {
        protected override IRepository<CheckersState> Repository => Ctx.CheckersStateRepository;

        public EditModel(IRepositoryContext ctx) : base(ctx)
        {
        }
    }
}
