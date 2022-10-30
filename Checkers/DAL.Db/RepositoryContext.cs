namespace DAL.Db;

public class RepositoryContext : IRepositoryContext
{
    public RepositoryContext(AppDbContext dbContext)
    {
        CheckersGameRepository = new CheckersGameRepository(dbContext);
        CheckersOptionsRepository = new CheckersOptionsRepository(dbContext);
        CheckersStateRepository = new CheckersStateRepository(dbContext);
    }

    public ICheckersGameRepository CheckersGameRepository { get; }
    public ICheckersOptionsRepository CheckersOptionsRepository { get; }
    public ICheckersStateRepository CheckersStateRepository { get; }
    public string Name => "Sqlite DB";
}