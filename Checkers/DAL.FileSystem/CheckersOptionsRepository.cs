using Domain;

namespace DAL.FileSystem;

public class CheckersOptionsRepository : AbstractFileSystemRepository<CheckersOptions>, ICheckersOptionsRepository
{
    protected override string RepositoryName => "CheckersOptions";

    public CheckersOptionsRepository(RepositoryContext repositoryContext) : base(repositoryContext)
    {
        foreach (var checkerOptions in DefaultCheckersOptions.DefaultOptions)
        {
            if (!ExistsEquivalent(checkerOptions))
            {
                Add(checkerOptions);
            }
        }

        PreSaveActions.Add(co => co.UpdateLastModified());
    }

    private bool ExistsEquivalent(CheckersOptions checkersOptions)
    {
        return GetAll().ToList().Exists(other => other.IsEquivalent(checkersOptions));
    }
}