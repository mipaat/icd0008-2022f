using Domain;

namespace DAL.Db;

public class RepositoryContext : IRepositoryContext
{
    public RepositoryContext(AppDbContext dbContext)
    {
        CheckersGameRepository = new CheckersGameRepository(dbContext, this);
        CheckersRulesetRepository = new CheckersRulesetRepository(dbContext, this);
        CheckersStateRepository = new CheckersStateRepository(dbContext, this);
    }

    public ICheckersGameRepository CheckersGameRepository { get; }
    public ICheckersRulesetRepository CheckersRulesetRepository { get; }
    public ICheckersStateRepository CheckersStateRepository { get; }
    private List<IRepository> Repositories => new(){CheckersGameRepository, CheckersStateRepository, CheckersRulesetRepository};
    public string Name => "Sqlite DB";
    public IRepository<T>? GetRepo<T>(Type type) where T : class, IDatabaseEntity, new()
    {
        return Repositories.Find(repo => repo.EntityType == type) as IRepository<T>;
    }

    public IRepository<T>? GetRepo<T>(T entity) where T : class, IDatabaseEntity, new()
    {
        return GetRepo<T>(entity.GetType());
    }
}