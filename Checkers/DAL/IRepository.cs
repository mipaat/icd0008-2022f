using DAL.Filters;
using Domain;

namespace DAL;

public interface IRepository
{
    Type EntityType { get; }
}

public interface IRepository<T> : IRepository
    where T : class, IDatabaseEntity, new()
{
    ICollection<T> GetAll(params FilterFunc<T>[] filters);

    T? GetById(int id);

    void Add(T entity);

    void Update(T entity);

    void Upsert(T entity);

    T? Remove(int id);

    T Remove(T entity);

    bool Exists(int id);

    void Refresh(T entity);
}