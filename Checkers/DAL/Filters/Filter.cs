using Domain;

namespace DAL.Filters;

public abstract class Filter<T> : IFilter<T> where T : IDatabaseEntity
{
    protected Filter(string identifier, FilterFunc<T> filterFunc, string? displayString = null)
    {
        Identifier = identifier;
        FilterFunc = filterFunc;
        DisplayString = displayString;
    }

    public string Identifier { get; }
    public string? DisplayString { get; }
    public FilterFunc<T> FilterFunc { get; }

    public IQueryable<T> Invoke(IQueryable<T> arg)
    {
        return FilterFunc(arg);
    }

    public static TFilter? Construct<TFilter>(string? identifier, List<TFilter> values) where TFilter : IFilter<T>
    {
        return values.Find(f => f.Identifier == identifier);
    }

    public override string ToString()
    {
        return DisplayString ?? Identifier;
    }
}