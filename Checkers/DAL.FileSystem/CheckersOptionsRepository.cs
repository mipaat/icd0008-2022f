using Domain;

namespace DAL.FileSystem;

public class CheckersOptionsRepository: AbstractFileSystemRepository<CheckersOptions>, ICheckersOptionsRepository
{
    protected override string RepositoryName => "CheckersOptions";

    private static readonly List<CheckersOptions> BuiltInOptions = new()
    {
        new CheckersOptions
        {
            Title = "Classic (8x8)",
            BuiltIn = true,
        },
        new CheckersOptions
        {
            Title = "10x10",
            BuiltIn = true,
            Width = 10,
            Height = 10,
        },
        new CheckersOptions
        {
            Title = "No captures required",
            BuiltIn = true,
            MustCapture = false
        },
        new CheckersOptions
        {
            Title = "Backwards jumps allowed",
            BuiltIn = true,
            CanJumpBackwards = true,
            CanJumpBackwardsDuringMultiCapture = true
        }
    };

    public CheckersOptionsRepository(RepositoryContext repositoryContext) : base(repositoryContext)
    {
        foreach (var checkerOptions in BuiltInOptions)
        {
            if (!ExistsEquivalent(checkerOptions))
            {
                Add(checkerOptions);
            }
        }
    }

    private bool ExistsEquivalent(CheckersOptions checkersOptions)
    {
        return GetAll().Exists(other => other.IsEquivalent(checkersOptions));
    }
}