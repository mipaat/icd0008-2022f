using Domain;

namespace DAL;

public interface IRepository<T>
    where T : class, IDatabaseEntity
{
    List<T> GetAll();

    T GetById(int id);

    void Add(T entity);

    void Update(T entity);

    void Upsert(T entity);

    T Remove(int id);

    bool Exists(int id);
}