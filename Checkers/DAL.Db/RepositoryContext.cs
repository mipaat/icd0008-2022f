namespace DAL.Db;

public class RepositoryContext : IRepositoryContext
{
    public RepositoryContext(AppDbContext dbContext)
    {
        CheckersGameRepository = new CheckersGameRepository(dbContext);
        CheckersRulesetRepository = new CheckersRulesetRepository(dbContext);
        CheckersStateRepository = new CheckersStateRepository(dbContext);
    }

    public ICheckersGameRepository CheckersGameRepository { get; }
    public ICheckersRulesetRepository CheckersRulesetRepository { get; }
    public ICheckersStateRepository CheckersStateRepository { get; }
    public string Name => "Sqlite DB";
}