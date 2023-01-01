using Domain;

namespace DAL.FileSystem;

public class RepositoryContext : IRepositoryContext
{
    public RepositoryContext()
    {
        CheckersGameRepository = new CheckersGameRepository(this);
        CheckersRulesetRepository = new CheckersRulesetRepository(this);
        CheckersStateRepository = new CheckersStateRepository(this);
    }

    public ICheckersGameRepository CheckersGameRepository { get; }
    public ICheckersRulesetRepository CheckersRulesetRepository { get; }
    public ICheckersStateRepository CheckersStateRepository { get; }
    private List<IRepository> Repositories => new(){CheckersGameRepository, CheckersStateRepository, CheckersRulesetRepository};
    public string Name => "FileSystem";
    public IRepository<T>? GetRepo<T>(Type type) where T : class, IDatabaseEntity, new()
    {
        return Repositories.Find(repo => repo.GetType() == type) as IRepository<T>;
    }

    public IRepository<T>? GetRepo<T>(T entity) where T : class, IDatabaseEntity, new()
    {
        return GetRepo<T>(entity.GetType());
    }
}