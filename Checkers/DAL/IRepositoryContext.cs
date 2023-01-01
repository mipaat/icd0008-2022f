using Domain;

namespace DAL;

public interface IRepositoryContext
{
    public ICheckersGameRepository CheckersGameRepository { get; }
    public ICheckersRulesetRepository CheckersRulesetRepository { get; }
    public ICheckersStateRepository CheckersStateRepository { get; }

    public string Name { get; }
    public IRepository<T>? GetRepo<T>(Type type) where T : class, IDatabaseEntity, new();
    public IRepository<T>? GetRepo<T>(T entity) where T : class, IDatabaseEntity, new();
}