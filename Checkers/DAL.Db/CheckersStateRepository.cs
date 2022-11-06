using Domain;
using Microsoft.EntityFrameworkCore;

namespace DAL.Db;

public class CheckersStateRepository : AbstractDbRepository<CheckersState>, ICheckersStateRepository
{
    public CheckersStateRepository(AppDbContext dbContext) : base(dbContext)
    {
        PreSaveActions.Add(cs => cs.SerializeGamePieces());
        PreFetchActions.Add(cs => cs.DeserializeGamePieces());
    }

    protected override DbSet<CheckersState> ThisDbSet => DbContext.CheckersStates;

    public ICollection<CheckersState> GetByCheckersGameId(int checkersGameId)
    {
        return ThisDbSet
            .Where(cs => checkersGameId.Equals(cs.CheckersGameId))
            .OrderBy(cs => cs.CreatedAt)
            .ToList();
    }
}