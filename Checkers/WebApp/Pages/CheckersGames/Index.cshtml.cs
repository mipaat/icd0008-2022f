using Common;
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

        [BindProperty(SupportsGet = true)] public int? AiQuery { get; set; }
        [BindProperty(SupportsGet = true)] public string? RulesetNameQuery { get; set; }
        [BindProperty(SupportsGet = true)] public string? PlayerNameQuery { get; set; }
        [BindProperty(SupportsGet = true)] public ECompletionFilter? CompletionFilter { get; set; }

        public IActionResult OnGet()
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

            if (AiQuery != null)
            {
                if (EnumUtils.TryGetEnum(AiQuery, out EAiType? aiType))
                {
                    result = result.Where(cg => cg.BlackAiType == aiType || cg.WhiteAiType == aiType);
                } else if (EnumUtils.TryGetEnum(AiQuery, out EAiTypeExtended? aiTypeExtended))
                {
                    result = aiTypeExtended switch
                    {
                        EAiTypeExtended.All => result,
                        EAiTypeExtended.NoAi => result.Where(cg => cg.BlackAiType == null && cg.WhiteAiType == null),
                        _ => result
                    };
                }
            }

            result = CompletionFilter switch
            {
                ECompletionFilter.Ongoing => result.Where(cg => !cg.Ended),
                ECompletionFilter.Finished => result.Where(cg => cg.Ended),
                ECompletionFilter.Tied => result.Where(cg => cg.Tied),
                ECompletionFilter.All => result,
                _ => result.Where(cg => !cg.Ended)
            };

            Entities = result.ToList();

            return Page();
        }

        protected override IRepository<CheckersGame> Repository => Ctx.CheckersGameRepository;
    }
}