using DAL.Filters;
using Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace DAL.Db;

public sealed class CheckersStateRepository : AbstractDbRepository<CheckersState>, ICheckersStateRepository
{
    public CheckersStateRepository(AppDbContext dbContext, IRepositoryContext repoContext) : base(dbContext,
        repoContext)
    {
        PreSaveActions.Add(cs =>
        {
            if (cs.GamePieces != null) cs.SerializeGamePieces();
        });
        PreFetchActions.Add(cs => cs.DeserializeGamePieces());
    }

    protected override DbSet<CheckersState> ThisDbSet => DbContext.CheckersStates;

    protected override List<FilterFunc<CheckersState>> DefaultFilters => new()
    {
        new FilterFunc<CheckersState>(iq => iq.OrderByDescending(cs => cs.CreatedAt), true)
    };

    public ICollection<CheckersState> GetByCheckersGameId(int checkersGameId)
    {
        return GetAll(new FilterFunc<CheckersState>(
            iq => iq.Where(cs => cs.CheckersGameId == checkersGameId), true));
    }
}