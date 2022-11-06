using Domain;

namespace DAL.FileSystem;

public class CheckersGameRepository : AbstractFileSystemRepository<CheckersGame>, ICheckersGameRepository
{
    protected override string RepositoryName => "CheckersGame";

    public CheckersGameRepository(RepositoryContext repositoryContext) : base(repositoryContext)
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
        RepositoryContext.CheckersRulesetRepository.Upsert(entity.CheckersRuleset);
        entity.CheckersRulesetId = entity.CheckersRuleset.Id;

        foreach (var checkersState in entity.AssertSufficientCheckersStates())
        {
            checkersState.CheckersGameId = entity.Id;
            RepositoryContext.CheckersStateRepository.Upsert(checkersState);
        }

        return base.Serialize(entity);
    }

    public new CheckersGame Remove(int id)
    {
        foreach (var checkersState in RepositoryContext.CheckersStateRepository.GetByCheckersGameId(id))
        {
            RepositoryContext.CheckersStateRepository.Remove(checkersState.Id);
        }

        var result = base.Remove(id);

        RepositoryContext.CheckersRulesetRepository.Remove(result.CheckersRulesetId);

        return result;
    }

    public new ICollection<CheckersGame> GetAll()
    {
        return base.GetAll().OrderByDescending(cg => cg.CurrentCheckersState!.CreatedAt).ToList();
    }
}