namespace DAL.FileSystem;

public class RepositoryContext
{
    public RepositoryContext()
    {
        CheckersGameRepository = new CheckersGameRepository(this);
        CheckersOptionsRepository = new CheckersOptionsRepository(this);
        CheckersStateRepository = new CheckersStateRepository(this);
    }

    public ICheckersGameRepository CheckersGameRepository { get; }
    public ICheckersOptionsRepository CheckersOptionsRepository { get; }
    public ICheckersStateRepository CheckersStateRepository { get; }
}