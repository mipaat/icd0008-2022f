using DAL.Filters;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace DAL.Db;

public abstract class AbstractDbRepository<T> : IRepository<T> where T : class, IDatabaseEntity, new()
{
    protected readonly AppDbContext DbContext;
    protected readonly IRepositoryContext RepositoryContext;

    protected AbstractDbRepository(AppDbContext dbContext, IRepositoryContext repoContext)
    {
        DbContext = dbContext;
        RepositoryContext = repoContext;
        PreSaveActions = new List<Action<T>>();
        PreFetchActions = new List<Action<T>>();
    }

    protected ICollection<Action<T>> PreSaveActions { get; }
    protected ICollection<Action<T>> PreFetchActions { get; }

    protected abstract DbSet<T> ThisDbSet { get; }
    protected IQueryable<T> Queryable => ThisDbSet;

    protected ICollection<T> GetAll(IQueryable<T> queryable, params FilterFunc<T>[] filters)
    {
        var realFilters = DefaultFilters?.ToList() ?? new List<FilterFunc<T>>();
        realFilters.AddRange(filters);
        return RunFilters(
                RunPreFetchActions(RunFilters(queryable, FilterFunc.GetQueryConvertible(realFilters))),
                FilterFunc.GetNonQueryConvertible(realFilters))
            .ToList();
    }

    public virtual ICollection<T> GetAll(params FilterFunc<T>[] filters)
    {
        return GetAll(Queryable, filters);
    }

    public virtual T? GetById(int id)
    {
        var entity = Queryable.FirstOrDefault(t => id.Equals(t.Id));
        return entity == null ? entity : RunPreFetchActions(entity);
    }

    public virtual void Add(T entity)
    {
        if (Exists(entity.Id))
            throw new ArgumentException(
                $"Can't add entity {typeof(T).Name} with ID {entity.Id} - ID already exists!");
        ThisDbSet.Add(RunPreSaveActions(entity));
        DbContext.SaveChanges();
    }

    public void Update(T entity)
    {
        if (!Exists(entity.Id))
            throw new ArgumentException($"Can't update entity {typeof(T).Name} with ID {entity.Id} - ID not found!");
        ThisDbSet.Update(RunPreSaveActions(entity));
        DbContext.SaveChanges();
    }

    public virtual void Upsert(T entity)
    {
        if (Exists(entity.Id))
            Update(entity);
        else
            Add(entity);
    }

    public T? Remove(int id)
    {
        var entity = GetById(id);
        return entity == null ? entity : Remove(entity);
    }

    public virtual T Remove(T entity)
    {
        var removedEntity = ThisDbSet.Remove(RunPreSaveActions(entity)).Entity;
        DbContext.SaveChanges();
        return removedEntity;
    }

    public bool Exists(int id)
    {
        return Queryable.Any(t => id.Equals(t.Id));
    }

    public Type EntityType => typeof(T);

    private static T RunActions(T entity, IEnumerable<Action<T>> actions)
    {
        foreach (var action in actions) action(entity);

        return entity;
    }

    private static void RunActions(IEnumerable<T> entities, ICollection<Action<T>> actions)
    {
        foreach (var entity in entities) RunActions(entity, actions);
    }

    private T RunPreSaveActions(T entity)
    {
        return RunActions(entity, PreSaveActions);
    }

    protected T RunPreFetchActions(T entity)
    {
        return RunActions(entity, PreFetchActions);
    }

    protected IEnumerable<T> RunPreFetchActions(IQueryable<T> entities)
    {
        return RunPreFetchActions(entities.AsEnumerable());
    }

    protected IEnumerable<T> RunPreFetchActions(IEnumerable<T> entities)
    {
        var result = entities.ToList();
        RunActions(result, PreFetchActions);
        return result;
    }

    protected IEnumerable<T> RunFilters(IEnumerable<T> enumerable, IEnumerable<FilterFunc<T>> filters)
    {
        return RunFilters(enumerable.AsQueryable(), filters);
    }

    protected IQueryable<T> RunFilters(IQueryable<T> queryable, IEnumerable<FilterFunc<T>> filters)
    {
        var fetched = false;
        foreach (var filter in filters)
        {
            if (!filter.IsConvertibleToDbQuery && !fetched)
            {
                fetched = true;
                queryable = queryable.AsEnumerable().AsQueryable();
            }
            queryable = filter.Filter(queryable);
        }

        return queryable;
    }

    protected virtual List<FilterFunc<T>>? DefaultFilters { get; } = null;
}