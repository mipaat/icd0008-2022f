using Domain;

namespace DAL.FileSystem;

public sealed class CheckersGameRepository : AbstractFileSystemRepository<CheckersGame>, ICheckersGameRepository
{
    protected override string RepositoryName => "CheckersGame";

    public CheckersGameRepository(IRepositoryContext repositoryContext) : base(repositoryContext)
    {
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

    public override CheckersGame Remove(CheckersGame entity)
    {
        foreach (var checkersState in RepositoryContext.CheckersStateRepository.GetByCheckersGameId(entity.Id))
        {
            RepositoryContext.CheckersStateRepository.Remove(checkersState);
        }

        var result = base.Remove(entity);

        RepositoryContext.CheckersRulesetRepository.Remove(result.CheckersRulesetId);

        return result;
    }

    public override ICollection<CheckersGame> GetAll()
    {
        return base.GetAll().OrderByDescending(cg => cg.CurrentCheckersState!.CreatedAt).ToList();
    }

    public CheckersGame? GetById(int id, bool noTracking, bool includeAllCheckersStates)
    {
        // Ignoring includeAllCheckersStates because that option is meant for optimizing DB queries.
        return GetById(id, noTracking);
    }

    public ICollection<CheckersGame> GetAll(bool includeAllCheckersStates)
    {
        // Ignoring includeAllCheckersStates because that option is meant for optimizing DB queries.
        return GetAll();
    }
}