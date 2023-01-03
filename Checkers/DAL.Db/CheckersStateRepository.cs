using Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace DAL.Db;

public sealed class CheckersStateRepository : AbstractDbRepository<CheckersState>, ICheckersStateRepository
{
    public CheckersStateRepository(AppDbContext dbContext, IRepositoryContext repoContext) : base(dbContext,
        repoContext)
    {
        PreSaveActions.Add(cs => cs.SerializeGamePieces());
        PreFetchActions.Add(cs => cs.DeserializeGamePieces());
    }

    protected override DbSet<CheckersState> ThisDbSet => DbContext.CheckersStates;

    public ICollection<CheckersState> GetByCheckersGameId(int checkersGameId)
    {
        return RunPreFetchActions(Queryable
                .Where(cs => checkersGameId.Equals(cs.CheckersGameId))
                .OrderBy(cs => cs.CreatedAt))
            .ToList();
    }
}