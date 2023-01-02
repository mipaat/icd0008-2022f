using Common;
using DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using GameBrain;
using WebApp.MyLibraries.PageModels;

namespace WebApp.Pages.CheckersGames
{
    public class CreateModel : PageModelDb
    {
        public IActionResult OnGet()
        {
            return Page();
        }

        public IEnumerable<SelectListItem> CheckersRulesets
        {
            get
            {
                var result = new List<SelectListItem>();
                foreach (var checkersRuleset in Ctx.CheckersRulesetRepository.GetAllSaved())
                {
                    result.Add(new SelectListItem(checkersRuleset.TitleText, checkersRuleset.Id.ToString()));
                }

                return result;
            }
        }

        [BindProperty] public string? WhitePlayerName { get; set; }
        [BindProperty] public string? BlackPlayerName { get; set; }
        [BindProperty] public EAiType? WhiteAiType { get; set; }
        [BindProperty] public EAiType? BlackAiType { get; set; }
        [BindProperty] public int CheckersRulesetId { get; set; }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var checkersRuleset = Ctx.CheckersRulesetRepository.GetById(CheckersRulesetId)?.GetClone(false);

            if (checkersRuleset == null) return Page();

            var checkersBrain =
                new CheckersBrain(checkersRuleset, WhitePlayerName, BlackPlayerName, WhiteAiType, BlackAiType);
            var checkersGame = checkersBrain.CheckersGame;
            Ctx.CheckersGameRepository.Add(checkersGame);

            return RedirectToPage("./Launch", new { id = checkersGame.Id });
        }

        public CreateModel(IRepositoryContext ctx) : base(ctx)
        {
        }
    }
}