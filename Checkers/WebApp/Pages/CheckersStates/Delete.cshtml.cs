using DAL;
using Domain.Model;
using WebApp.MyLibraries.PageModels;

namespace WebApp.Pages.CheckersStates;

public class DeleteModel : DeleteModel<CheckersState>
{
    public DeleteModel(IRepositoryContext ctx) : base(ctx)
    {
    }

    protected override IRepository<CheckersState> Repository => Ctx.CheckersStateRepository;
}