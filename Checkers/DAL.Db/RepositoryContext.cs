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
    public string Name => "Sqlite DB";
}