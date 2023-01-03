using Domain.Model;
using Domain.Model.Helpers;

namespace DAL.Filters;

public class AiTypeFilter : SimpleFilter<CheckersGame>
{
    public static readonly AiTypeFilter Random = new(nameof(Random),
        games => games.Where(game => game.BlackAiType == EAiType.Random || game.WhiteAiType == EAiType.Random));

    public static readonly AiTypeFilter Simple = new(nameof(Simple),
        games => games.Where(game => game.BlackAiType == EAiType.Simple || game.WhiteAiType == EAiType.Simple));

    public static readonly AiTypeFilter SimpleMinMax = new(nameof(SimpleMinMax),
        games => games.Where(game =>
            game.BlackAiType == EAiType.SimpleMinMax || game.WhiteAiType == EAiType.SimpleMinMax));


    public static readonly AiTypeFilter All = new(nameof(All), games => games);

    public static readonly AiTypeFilter None = new(nameof(None),
        games => games.Where(game => game.BlackAiType == null && game.WhiteAiType == null),
        "No AI");

    private AiTypeFilter(string identifier, FilterFuncInner<CheckersGame> filterFunc, string? displayString = null) :
        base(identifier, filterFunc, true, displayString)
    {
    }

    public static AiTypeFilter Default => All;

    public static List<AiTypeFilter> Values => new() { All, Random, Simple, SimpleMinMax, None };
}