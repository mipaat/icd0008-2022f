using DAL;
using Microsoft.AspNetCore.Mvc;
using Domain;
using WebApp.MyLibraries.PageModels;

namespace WebApp.Pages.CheckersRulesets
{
    public class IndexModel : IndexModel<CheckersRuleset>
    {
        [BindProperty(SupportsGet = true)] public bool ShowNonSaved { get; set; }

        public override IActionResult OnGet()
        {
            Entities =
                (ShowNonSaved ? Ctx.CheckersRulesetRepository.GetAll() : Ctx.CheckersRulesetRepository.GetAllSaved())
                .ToList();
            return Page();
        }

        protected override IRepository<CheckersRuleset> Repository => Ctx.CheckersRulesetRepository;

        public IndexModel(IRepositoryContext ctx) : base(ctx)
        {
        }
    }
}