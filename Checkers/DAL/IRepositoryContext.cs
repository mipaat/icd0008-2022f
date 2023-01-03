using Domain;

namespace DAL;

public interface IRepositoryContext : IDisposable
{
    public ICheckersGameRepository CheckersGameRepository { get; }
    public ICheckersRulesetRepository CheckersRulesetRepository { get; }
    public ICheckersStateRepository CheckersStateRepository { get; }

    public IRepository<T>? GetRepo<T>(Type type) where T : class, IDatabaseEntity, new();
    public IRepository<T>? GetRepo<T>(T entity) where T : class, IDatabaseEntity, new();
}