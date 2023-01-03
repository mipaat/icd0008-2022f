using Domain;

namespace DAL.Filters;

public abstract class SimpleFilter<T> : IFilter<T> where T : IDatabaseEntity
{
    protected SimpleFilter(string identifier, FilterFuncInner<T> filterFunc, bool isConvertibleToDbQuery = true, string? displayString = null)
    {
        Identifier = identifier;
        FilterFunc = new FilterFunc<T>(filterFunc, isConvertibleToDbQuery);
        DisplayString = displayString;
    }

    public string Identifier { get; }
    public string? DisplayString { get; }
    public FilterFunc<T> FilterFunc { get; }

    public static TFilter? Construct<TFilter>(string? identifier, List<TFilter> values) where TFilter : IFilter<T>
    {
        return values.Find(f => f.Identifier == identifier);
    }

    public override string ToString()
    {
        return DisplayString ?? Identifier;
    }
}