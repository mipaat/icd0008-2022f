namespace DAL;

public interface IRepositoryContextFactory
{
    public IRepositoryContext CreateRepositoryContext();
    public string Name { get; }
}