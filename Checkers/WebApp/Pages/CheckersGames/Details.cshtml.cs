using DAL;
using Domain.Model;
using WebApp.MyLibraries.PageModels;

namespace WebApp.Pages.CheckersGames;

public class DetailsModel : EntityModel<CheckersGame>
{
    public DetailsModel(IRepositoryContext ctx) : base(ctx)
    {
    }

    protected override IRepository<CheckersGame> Repository => Ctx.CheckersGameRepository;
}