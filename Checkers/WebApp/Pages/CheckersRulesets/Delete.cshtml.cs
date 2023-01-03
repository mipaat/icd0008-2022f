using DAL;
using Domain.Model;
using WebApp.MyLibraries.PageModels;

namespace WebApp.Pages.CheckersRulesets;

public class DeleteModel : DeleteModel<CheckersRuleset>
{
    public DeleteModel(IRepositoryContext ctx) : base(ctx)
    {
    }

    protected override IRepository<CheckersRuleset> Repository => Ctx.CheckersRulesetRepository;
}