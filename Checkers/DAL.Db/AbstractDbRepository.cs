using Domain;
using Microsoft.EntityFrameworkCore;

namespace DAL.Db;

public abstract class AbstractDbRepository<T> : IRepository<T> where T : class, IDatabaseEntity
{
    protected readonly AppDbContext DbContext;
    protected readonly IRepositoryContext RepositoryContext;
    protected ICollection<Action<T>> PreSaveActions { get; }
    protected ICollection<Action<T>> PreFetchActions { get; }

    protected AbstractDbRepository(AppDbContext dbContext, IRepositoryContext repoContext)
    {
        DbContext = dbContext;
        RepositoryContext = repoContext;
        PreSaveActions = new List<Action<T>>();
        PreFetchActions = new List<Action<T>>();
    }

    private static T RunActions(T entity, IEnumerable<Action<T>> actions)
    {
        foreach (var action in actions)
        {
            action(entity);
        }

        return entity;
    }

    private static IQueryable<T> RunActions(IQueryable<T> entities, ICollection<Action<T>> actions)
    {
        foreach (var entity in entities)
        {
            RunActions(entity, actions);
        }

        return entities;
    }

    private T RunPreSaveActions(T entity)
    {
        return RunActions(entity, PreSaveActions);
    }

    protected T RunPreFetchActions(T entity)
    {
        return RunActions(entity, PreFetchActions);
    }

    protected IQueryable<T> RunPreSaveActions(IQueryable<T> entities)
    {
        return RunActions(entities, PreSaveActions);
    }

    protected IQueryable<T> RunPreFetchActions(IQueryable<T> entities)
    {
        return RunActions(entities, PreFetchActions);
    }

    protected abstract DbSet<T> ThisDbSet { get; }

    public ICollection<T> GetAll()
    {
        return RunPreFetchActions(ThisDbSet).ToList();
    }

    public T? GetById(int id)
    {
        var entity = ThisDbSet.FirstOrDefault(t => id.Equals(t.Id));
        return entity == null ? entity : RunPreFetchActions(entity);
    }

    public void Add(T entity)
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

    public void Upsert(T entity)
    {
        ThisDbSet.Update(RunPreSaveActions(entity));
        DbContext.SaveChanges();
    }

    public T? Remove(int id)
    {
        var entity = GetById(id);
        return entity == null ? entity : Remove(entity);
    }

    public T Remove(T entity)
    {
        var removedEntity = ThisDbSet.Remove(RunPreSaveActions(entity)).Entity;
        DbContext.SaveChanges();
        return removedEntity;
    }

    public bool Exists(int id)
    {
        return ThisDbSet.Any(t => id.Equals(t.Id));
    }

    public void Refresh(T entity)
    {
        DbContext.Entry(entity).Reload();
    }
}