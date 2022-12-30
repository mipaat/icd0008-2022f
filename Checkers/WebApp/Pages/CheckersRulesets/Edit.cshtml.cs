using DAL;
using Domain;
using WebApp.MyLibraries.PageModels;

namespace WebApp.Pages.CheckersRulesets
{
    public class EditModel : EditModel<CheckersRuleset>
    {
        protected override IRepository<CheckersRuleset> Repository => Ctx.CheckersRulesetRepository;

        public EditModel(IRepositoryContext ctx) : base(ctx)
        {
        }
    }
}