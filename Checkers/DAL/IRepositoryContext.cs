namespace DAL;

public interface IRepositoryContext
{
    public ICheckersGameRepository CheckersGameRepository { get; }
    public ICheckersRulesetRepository CheckersRulesetRepository { get; }
    public ICheckersStateRepository CheckersStateRepository { get; }
    
    public string Name { get; }
}