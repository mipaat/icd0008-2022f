using DAL;
using Domain.Model;
using WebApp.MyLibraries.PageModels;

namespace WebApp.Pages.CheckersStates;

public class IndexModel : IndexModel<CheckersState>
{
    public IndexModel(IRepositoryContext ctx) : base(ctx)
    {
    }

    protected override IRepository<CheckersState> Repository => Ctx.CheckersStateRepository;
}