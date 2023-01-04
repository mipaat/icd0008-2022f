using DAL.Filters;
using Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace DAL.Db;

public sealed class CheckersGameRepository : AbstractDbRepository<CheckersGame>, ICheckersGameRepository
{
    public CheckersGameRepository(AppDbContext dbContext, IRepositoryContext repoContext) : base(dbContext, repoContext)
    {
        PreSaveActions.Add(cg =>
        {
            if (cg.CheckersStates == null) return;
            foreach (var checkersState in cg.CheckersStates) checkersState.SerializeGamePieces();
        });
        PreFetchActions.Add(cg =>
        {
            foreach (var checkersState in cg.AssertSufficientCheckersStates()) checkersState.DeserializeGamePieces();
        });
    }

    protected override DbSet<CheckersGame> ThisDbSet => DbContext.CheckersGames;

    public override ICollection<CheckersGame> GetAll(
        params FilterFunc<CheckersGame>[] filters)
    {
        return GetAll(false, filters);
    }

    protected override List<FilterFunc<CheckersGame>> DefaultFilters => new()
    {
        new FilterFunc<CheckersGame>(iq => iq.OrderByDescending(cg => cg.CurrentCheckersState!.CreatedAt), false)
    };

    public ICollection<CheckersGame> GetAll(bool includeAllCheckersStates,
        params FilterFunc<CheckersGame>[] filters)
    {
        var checkersStatesIncluded = includeAllCheckersStates
            ? Queryable.Include(cg => cg.CheckersStates)!
            : Queryable.Include(cg => cg.CheckersStates!.OrderByDescending(cs => cs.CreatedAt).Take(1));
        var checkersRulesetIncluded = checkersStatesIncluded.Include(cg => cg.CheckersRuleset);
        return GetAll(checkersRulesetIncluded, filters);
    }

    public override CheckersGame? GetById(int id)
    {
        return GetById(id, false);
    }

    public CheckersGame? GetById(int id, bool includeAllCheckersStates)
    {
        var checkersStatesIncluded = includeAllCheckersStates
            ? Queryable.Include(cg => cg.CheckersStates)!
            : Queryable.Include(cg => cg.CheckersStates!.OrderByDescending(cs => cs.CreatedAt).Take(1));
        var entity = checkersStatesIncluded
            .Include(cg => cg.CheckersRuleset)
            .FirstOrDefault(cg => id.Equals(cg.Id));
        return entity == null ? entity : RunPreFetchActions(entity);
    }

    public override CheckersGame Remove(CheckersGame entity)
    {
        var removedEntity = base.Remove(entity);

        foreach (var checkersState in RepositoryContext.CheckersStateRepository.GetByCheckersGameId(entity.Id))
            RepositoryContext.CheckersStateRepository.Remove(checkersState);

        RepositoryContext.CheckersRulesetRepository.Remove(removedEntity.CheckersRulesetId);
        return removedEntity;
    }
}