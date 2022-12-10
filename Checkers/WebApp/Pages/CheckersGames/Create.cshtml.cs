using DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Domain;
using GameBrain;

namespace WebApp.Pages.CheckersGames
{
    public class CreateModel : PageModel
    {
        private readonly IRepositoryContext _ctx;

        public CreateModel(IRepositoryContext ctx)
        {
            _ctx = ctx;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        public IEnumerable<SelectListItem> CheckersRulesets
        {
            get
            {
                var result = new List<SelectListItem>();
                foreach (var checkersRuleset in _ctx.CheckersRulesetRepository.GetAllSaved())
                {
                    result.Add(new SelectListItem(checkersRuleset.TitleText, checkersRuleset.Id.ToString()));
                }

                return result;
            }
        }

        public IEnumerable<SelectListItem> AiTypeOptions
        {
            get
            {
                var result = new List<SelectListItem>();
                var enumValues = Enum.GetValues(typeof(EAiType));
                result.Add(new SelectListItem("No AI", ""));
                foreach (var value in enumValues)
                {
                    result.Add(new SelectListItem(value.ToString(), ((int)value).ToString()));
                }

                return result;
            }
        }

        [BindProperty] public string WhitePlayerId { get; set; } = default!;
        [BindProperty] public string BlackPlayerId { get; set; } = default!;
        [BindProperty] public EAiType? WhiteAiType { get; set; }
        [BindProperty] public EAiType? BlackAiType { get; set; }
        [BindProperty] public int CheckersRulesetId { get; set; }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Task.FromResult<IActionResult>(Page());
            }

            var checkersRuleset = _ctx.CheckersRulesetRepository.GetById(CheckersRulesetId).GetClone(false);
            var checkersBrain =
                new CheckersBrain(checkersRuleset, WhitePlayerId, BlackPlayerId, WhiteAiType, BlackAiType);
            var checkersGame = checkersBrain.GetSaveGameState();
            _ctx.CheckersGameRepository.Add(checkersGame);

            return Task.FromResult<IActionResult>(RedirectToPage("./Play", new {id = checkersGame.Id}));
        }
    }
}