using DAL;
using Domain.Model;
using WebApp.MyLibraries.PageModels;

namespace WebApp.Pages.CheckersStates;

public class EditModel : EditModel<CheckersState>
{
    public EditModel(IRepositoryContext ctx) : base(ctx)
    {
    }

    protected override IRepository<CheckersState> Repository => Ctx.CheckersStateRepository;
}