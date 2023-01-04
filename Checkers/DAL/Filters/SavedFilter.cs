using Domain.Model;

namespace DAL.Filters;

public class SavedFilter : SimpleFilter<CheckersRuleset>
{
    public SavedFilter(string identifier, FilterFuncInner<CheckersRuleset> filterFunc,
        bool isConvertibleToDbQuery = true, string? displayString = null) : base(identifier, filterFunc,
        isConvertibleToDbQuery, displayString)
    {
    }

    public static readonly SavedFilter Saved = new(nameof(Saved),
        iq => iq.Where(cr => cr.Saved));

    public static readonly SavedFilter All = new(nameof(All), iq => iq);

    public static readonly SavedFilter NonSaved = new(nameof(NonSaved),
        iq => iq.Where(cr => !cr.Saved), displayString: "Not saved");

    public static SavedFilter Default => Saved;

    public static List<SavedFilter> Values => new() { Saved, All, NonSaved };
}