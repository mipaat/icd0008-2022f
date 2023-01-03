using DAL;
using Domain.Model;
using WebApp.MyLibraries.PageModels;

namespace WebApp.Pages.CheckersRulesets;

public class DetailsModel : EntityModel<CheckersRuleset>
{
    public DetailsModel(IRepositoryContext ctx) : base(ctx)
    {
    }

    protected override IRepository<CheckersRuleset> Repository => Ctx.CheckersRulesetRepository;
}