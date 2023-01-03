using Domain;

namespace DAL.Db;

public class RepositoryContext : IRepositoryContext
{
    private readonly AppDbContext _dbContext;
    
    public RepositoryContext()
    {
        _dbContext = AppDbContextFactory.CreateDbContext();
        CheckersGameRepository = new CheckersGameRepository(_dbContext, this);
        CheckersRulesetRepository = new CheckersRulesetRepository(_dbContext, this);
        CheckersStateRepository = new CheckersStateRepository(_dbContext, this);
    }

    private List<IRepository> Repositories => new()
        { CheckersGameRepository, CheckersStateRepository, CheckersRulesetRepository };

    public ICheckersGameRepository CheckersGameRepository { get; }
    public ICheckersRulesetRepository CheckersRulesetRepository { get; }
    public ICheckersStateRepository CheckersStateRepository { get; }

    public IRepository<T>? GetRepo<T>(Type type) where T : class, IDatabaseEntity, new()
    {
        return Repositories.Find(repo => repo.EntityType == type) as IRepository<T>;
    }

    public IRepository<T>? GetRepo<T>(T entity) where T : class, IDatabaseEntity, new()
    {
        return GetRepo<T>(entity.GetType());
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}