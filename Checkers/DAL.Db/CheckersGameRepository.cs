using Domain;
using Microsoft.EntityFrameworkCore;

namespace DAL.Db;

public class CheckersGameRepository : AbstractDbRepository<CheckersGame>, ICheckersGameRepository
{
    public CheckersGameRepository(AppDbContext dbContext) : base(dbContext)
    {
        PreSaveActions.Add(cg =>
        {
            foreach (var checkersState in cg.AssertSufficientCheckersStates())
            {
                checkersState.SerializeGamePieces();
            }
        });
        PreFetchActions.Add(cg =>
        {
            foreach (var checkersState in cg.AssertSufficientCheckersStates())
            {
                checkersState.DeserializeGamePieces();
            }
        });
    }

    public new ICollection<CheckersGame> GetAll()
    {
        return RunPreFetchActions(ThisDbSet
                .Include(cg => cg.CheckersStates)
                .Include(cg => cg.CheckersOptions)
            )
            .ToList()
            .OrderByDescending(cg=> cg.CurrentCheckersState!.CreatedAt)
            .ToList();
    }

    protected override DbSet<CheckersGame> ThisDbSet => DbContext.CheckersGames;
}