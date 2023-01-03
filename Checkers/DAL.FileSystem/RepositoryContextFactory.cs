namespace DAL.FileSystem;

public class RepositoryContextFactory : IRepositoryContextFactory
{
    public IRepositoryContext CreateRepositoryContext()
    {
        return new RepositoryContext();
    }

    public string Name => "FileSystem";
}