namespace DAL.Db;

public class RepositoryContextFactory : IRepositoryContextFactory
{
    public IRepositoryContext CreateRepositoryContext()
    {
        return new RepositoryContext();
    }
    
    public string Name => "Sqlite DB";
}