using DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Domain;

namespace WebApp.Pages.CheckersRulesets
{
    public class IndexModel : PageModel
    {
        private readonly IRepositoryContext _ctx;

        public IndexModel(IRepositoryContext ctx)
        {
            _ctx = ctx;
        }

        public IList<CheckersRuleset> CheckersRulesets { get; set; } = default!;

        [BindProperty(SupportsGet = true)] public bool ShowNonSaved { get; set; }

        public Task OnGet()
        {
            CheckersRulesets =
                (ShowNonSaved ? _ctx.CheckersRulesetRepository.GetAll() : _ctx.CheckersRulesetRepository.GetAllSaved())
                .ToList();
            return Task.CompletedTask;
        }
    }
}