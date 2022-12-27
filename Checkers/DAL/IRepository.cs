using Domain;

namespace DAL;

public interface IRepository<T>
    where T : class, IDatabaseEntity, new()
{
    ICollection<T> GetAll();

    T? GetById(int id, bool noTracking = false);

    void Add(T entity);

    void Update(T entity);

    void Upsert(T entity);

    T? Remove(int id);

    T Remove(T entity);

    bool Exists(int id);

    void Refresh(T entity);

    void RefreshPartial(T entity);
}