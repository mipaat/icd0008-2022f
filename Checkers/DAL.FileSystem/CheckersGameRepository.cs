using DAL.Filters;
using Domain.Model;

namespace DAL.FileSystem;

public sealed class CheckersGameRepository : AbstractFileSystemRepository<CheckersGame>, ICheckersGameRepository
{
    public CheckersGameRepository(IRepositoryContext repositoryContext) : base(repositoryContext)
    {
    }

    protected override string RepositoryName => "CheckersGame";

    public override CheckersGame Remove(CheckersGame entity)
    {
        foreach (var checkersState in RepositoryContext.CheckersStateRepository.GetByCheckersGameId(entity.Id))
            RepositoryContext.CheckersStateRepository.Remove(checkersState);

        var result = base.Remove(entity);

        RepositoryContext.CheckersRulesetRepository.Remove(result.CheckersRulesetId);

        return result;
    }

    public override ICollection<CheckersGame> GetAll(params FilterFunc<CheckersGame>[] filters)
    {
        return base.GetAll(filters).OrderByDescending(cg => cg.CurrentCheckersState!.CreatedAt).ToList();
    }

    public CheckersGame? GetById(int id, bool includeAllCheckersStates)
    {
        // Ignoring includeAllCheckersStates because that option is meant for optimizing DB queries.
        return GetById(id);
    }

    public ICollection<CheckersGame> GetAll(bool includeAllCheckersStates, params FilterFunc<CheckersGame>[] filters)
    {
        // Ignoring includeAllCheckersStates because that option is meant for optimizing DB queries.
        return GetAll(filters);
    }

    protected override CheckersGame Deserialize(string serializedData)
    {
        var result = base.Deserialize(serializedData);

        result.CheckersRuleset = RepositoryContext.CheckersRulesetRepository.GetById(result.CheckersRulesetId);
        result.CheckersStates = RepositoryContext.CheckersStateRepository.GetByCheckersGameId(result.Id);

        return result;
    }

    protected override string Serialize(CheckersGame entity)
    {
        RepositoryContext.CheckersRulesetRepository.Upsert(entity.CheckersRuleset!);
        entity.CheckersRulesetId = entity.CheckersRuleset!.Id;

        foreach (var checkersState in entity.AssertSufficientCheckersStates())
        {
            checkersState.CheckersGameId = entity.Id;
            RepositoryContext.CheckersStateRepository.Upsert(checkersState);
        }

        return base.Serialize(entity);
    }
}