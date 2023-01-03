using DAL;
using Domain.Model;
using WebApp.MyLibraries.PageModels;

namespace WebApp.Pages.CheckersGames;

public class EditModel : EditModel<CheckersGame>
{
    public EditModel(IRepositoryContext ctx) : base(ctx)
    {
    }

    protected override IRepository<CheckersGame> Repository => Ctx.CheckersGameRepository;
}