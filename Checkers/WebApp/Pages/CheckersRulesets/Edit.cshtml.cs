using DAL;
using Domain.Model;
using WebApp.MyLibraries.PageModels;

namespace WebApp.Pages.CheckersRulesets;

public class EditModel : EditModel<CheckersRuleset>
{
    public EditModel(IRepositoryContext ctx) : base(ctx)
    {
    }

    protected override IRepository<CheckersRuleset> Repository => Ctx.CheckersRulesetRepository;
}