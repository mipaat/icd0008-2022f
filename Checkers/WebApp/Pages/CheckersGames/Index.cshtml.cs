using DAL;
using Domain;
using Microsoft.AspNetCore.Mvc;
using WebApp.MyLibraries.PageModels;

namespace WebApp.Pages.CheckersGames
{
    public class IndexModel : RepositoryModel<CheckersGame>
    {
        public IndexModel(IRepositoryContext ctx) : base(ctx)
        {
        }

        public IList<CheckersGame> Entities { get; set; } = default!;

        private static bool StringContains(string? query, string? compare)
        {
            var normalizedQuery = query?.ToLower().Trim() ?? "";
            var normalizedCompare = compare?.ToLower().Trim() ?? "";
            return normalizedCompare.Contains(normalizedQuery);
        }
        
        [BindProperty(SupportsGet = true)] public EAiType? AiQuery { get; set; }
        [BindProperty(SupportsGet = true)] public string? RulesetNameQuery { get; set; }
        [BindProperty(SupportsGet = true)] public string? PlayerNameQuery { get; set; }
        [BindProperty(SupportsGet = true)] public ECompletionFilter? CompletionFilter { get; set; }

        public IActionResult OnGet(bool aiQuerySubmit = false)
        {
            IEnumerable<CheckersGame> result = Repository.GetAll();
            if (PlayerNameQuery != null)
            {
                result = result.Where(cg => StringContains(PlayerNameQuery, cg.WhitePlayerName)
                                            || StringContains(PlayerNameQuery, cg.BlackPlayerName));
            }

            if (RulesetNameQuery != null)
            {
                result = result.Where(cg => StringContains(RulesetNameQuery, cg.CheckersRuleset?.TitleText));
            }

            if (aiQuerySubmit)
            {
                result = AiQuery != null
                    ? result.Where(cg => cg.BlackAiType == AiQuery || cg.WhiteAiType == AiQuery)
                    : result.Where(cg => cg.BlackAiType == null && cg.WhiteAiType == null);
            }

            result = CompletionFilter switch
            {
                ECompletionFilter.Ongoing => result.Where(cg => !cg.Ended),
                ECompletionFilter.Finished => result.Where(cg => cg.Ended),
                ECompletionFilter.Tied => result.Where(cg => cg.Tied),
                _ => result
            };

            Entities = result.ToList();

            return Page();
        }

        protected override IRepository<CheckersGame> Repository => Ctx.CheckersGameRepository;
    }
}