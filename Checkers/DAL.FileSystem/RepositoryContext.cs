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
    public string Name => "FileSystem";
}