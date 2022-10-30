using Domain;

namespace DAL;

public interface IRepository<T>
    where T : class, IDatabaseEntity
{
    ICollection<T> GetAll();

    T GetById(int id);

    void Add(T entity);

    void Update(T entity);

    void Upsert(T entity);

    T Remove(int id);

    T Remove(T entity);

    bool Exists(int id);
}