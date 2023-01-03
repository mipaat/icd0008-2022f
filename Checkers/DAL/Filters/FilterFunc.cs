using Domain;

namespace DAL.Filters;

public delegate IQueryable<T> FilterFuncInner<T>(IQueryable<T> arg) where T : IDatabaseEntity;

public static class FilterFunc
{
    public static IEnumerable<FilterFunc<T>> GetQueryConvertible<T>(IEnumerable<FilterFunc<T>> filters)
        where T : IDatabaseEntity =>
        filters.Where(f => f.IsConvertibleToDbQuery);

    public static IEnumerable<FilterFunc<T>> GetNonQueryConvertible<T>(IEnumerable<FilterFunc<T>> filters)
        where T : IDatabaseEntity =>
        filters.Where(f => !f.IsConvertibleToDbQuery);
}

public class FilterFunc<T> where T : IDatabaseEntity
{
    public FilterFunc(FilterFuncInner<T> func, bool isConvertibleToDbQuery)
    {
        _func = func;
        IsConvertibleToDbQuery = isConvertibleToDbQuery;
    }

    private readonly FilterFuncInner<T> _func;
    public bool IsConvertibleToDbQuery { get; }
    public IQueryable<T> Filter(IQueryable<T> queryable) => _func.Invoke(queryable);
}