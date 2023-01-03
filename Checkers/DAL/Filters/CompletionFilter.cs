using Domain.Model;

namespace DAL.Filters;

public class CompletionFilter : Filter<CheckersGame>
{
    public static readonly CompletionFilter Ongoing = new(nameof(Ongoing), iq => iq.Where(cg => cg.EndedAt == null));
    public static readonly CompletionFilter All = new(nameof(All), iq => iq);
    public static readonly CompletionFilter Finished = new(nameof(Finished), iq => iq.Where(cg => cg.EndedAt != null));

    public static readonly CompletionFilter Tied = new(nameof(Tied),
        iq => iq.Where(cg => cg.EndedAt != null && cg.Winner == null));

    private CompletionFilter(string identifier, FilterFunc<CheckersGame> filterFunc, string? displayString = null) :
        base(identifier, filterFunc, displayString)
    {
    }

    public static CompletionFilter Default => Ongoing;
    public static List<CompletionFilter> Values => new() { Ongoing, All, Finished, Tied };
}