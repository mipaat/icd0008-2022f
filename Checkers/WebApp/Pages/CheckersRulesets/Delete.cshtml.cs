using DAL;
using Domain;
using WebApp.MyLibraries.PageModels;

namespace WebApp.Pages.CheckersRulesets
{
    public class DeleteModel : DeleteModel<CheckersRuleset>
    {
        protected override IRepository<CheckersRuleset> Repository => Ctx.CheckersRulesetRepository;

        public DeleteModel(IRepositoryContext ctx) : base(ctx)
        {
        }
    }
}