using Domain;
using Microsoft.EntityFrameworkCore;

namespace DAL.Db;

public class CheckersGameRepository : AbstractDbRepository<CheckersGame>, ICheckersGameRepository
{
    public CheckersGameRepository(AppDbContext dbContext, IRepositoryContext repoContext) : base(dbContext, repoContext)
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
                .Include(cg => cg.CheckersRuleset)
            )
            .ToList()
            .OrderByDescending(cg => cg.CurrentCheckersState!.CreatedAt)
            .ToList();
    }

    public new CheckersGame? GetById(int id)
    {
        var entity = ThisDbSet
            .Include(cg => cg.CheckersStates)
            .Include(cg => cg.CheckersRuleset)
            .FirstOrDefault(cg => id.Equals(cg.Id));
        return entity == null ? entity : RunPreFetchActions(entity);
    }

    protected override DbSet<CheckersGame> ThisDbSet => DbContext.CheckersGames;

    public new CheckersGame Remove(CheckersGame entity)
    {
        var removedEntity = base.Remove(entity);

        foreach (var checkersState in RepositoryContext.CheckersStateRepository.GetByCheckersGameId(entity.Id))
        {
            RepositoryContext.CheckersStateRepository.Remove(checkersState);
        }
        
        RepositoryContext.CheckersRulesetRepository.Remove(removedEntity.CheckersRulesetId);
        return removedEntity;
    }
}