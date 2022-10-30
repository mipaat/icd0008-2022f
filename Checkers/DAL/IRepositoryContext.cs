namespace DAL;

public interface IRepositoryContext
{
    public ICheckersGameRepository CheckersGameRepository { get; }
    public ICheckersOptionsRepository CheckersOptionsRepository { get; }
    public ICheckersStateRepository CheckersStateRepository { get; }
    
    public string Name { get; }
}