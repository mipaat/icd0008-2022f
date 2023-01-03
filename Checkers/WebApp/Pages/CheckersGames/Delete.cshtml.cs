using DAL;
using Domain.Model;
using WebApp.MyLibraries.PageModels;

namespace WebApp.Pages.CheckersGames;

public class DeleteModel : DeleteModel<CheckersGame>
{
    public DeleteModel(IRepositoryContext ctx) : base(ctx)
    {
    }

    protected override IRepository<CheckersGame> Repository => Ctx.CheckersGameRepository;
}