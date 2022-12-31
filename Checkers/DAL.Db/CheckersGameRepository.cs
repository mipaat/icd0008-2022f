using Domain;
using Microsoft.EntityFrameworkCore;

namespace DAL.Db;

public sealed class CheckersGameRepository : AbstractDbRepository<CheckersGame>, ICheckersGameRepository
{
    public CheckersGameRepository(AppDbContext dbContext, IRepositoryContext repoContext) : base(dbContext, repoContext)
    {
        PreSaveActions.Add(cg =>
        {
            if (cg.CheckersStates == null) return;
            foreach (var checkersState in cg.CheckersStates)
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

    public override ICollection<CheckersGame> GetAll()
    {
        return GetAll(false);
    }

    public ICollection<CheckersGame> GetAll(bool includeAllCheckersStates)
    {
        var checkersStatesIncluded = includeAllCheckersStates
            ? ThisDbSet.Include(cg => cg.CheckersStates)!
            : ThisDbSet.Include(cg => cg.CheckersStates!.OrderByDescending(cs => cs.CreatedAt).Take(1));
        return RunPreFetchActions(checkersStatesIncluded
                .Include(cg => cg.CheckersRuleset)
                .ToList()
            )
            .OrderByDescending(cg => cg.CurrentCheckersState!.CreatedAt)
            .ToList();
    }

    public override CheckersGame? GetById(int id)
    {
        return GetById(id, false);
    }

    public CheckersGame? GetById(int id, bool includeAllCheckersStates)
    {
        var dbSet = ThisDbSet;
        var checkersStatesIncluded = includeAllCheckersStates
            ? dbSet.Include(cg => cg.CheckersStates)!
            : dbSet.Include(cg => cg.CheckersStates!.OrderByDescending(cs => cs.CreatedAt).Take(1));
        var entity = checkersStatesIncluded
            .Include(cg => cg.CheckersRuleset)
            .FirstOrDefault(cg => id.Equals(cg.Id));
        return entity == null ? entity : RunPreFetchActions(entity);
    }

    protected override DbSet<CheckersGame> ThisDbSet => DbContext.CheckersGames;

    public override CheckersGame Remove(CheckersGame entity)
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