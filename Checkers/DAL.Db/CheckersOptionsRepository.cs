using Domain;
using Microsoft.EntityFrameworkCore;

namespace DAL.Db;

public class CheckersOptionsRepository : AbstractDbRepository<CheckersOptions>, ICheckersOptionsRepository
{
    public CheckersOptionsRepository(AppDbContext dbContext) : base(dbContext)
    {
        foreach (var checkersOptions in DefaultCheckersOptions.DefaultOptions)
        {
            if (!ExistsEquivalent(checkersOptions))
            {
                Add(checkersOptions);
            }
        }
    }

    protected override DbSet<CheckersOptions> ThisDbSet => DbContext.CheckersOptions;

    private bool ExistsEquivalent(CheckersOptions checkersOptions)
    {
        return ThisDbSet.AsEnumerable().Any(other => other.IsEquivalent(checkersOptions));
    }
}