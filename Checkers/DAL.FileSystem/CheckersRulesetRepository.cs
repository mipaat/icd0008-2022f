using Domain.Model;

namespace DAL.FileSystem;

public sealed class CheckersRulesetRepository : AbstractFileSystemRepository<CheckersRuleset>,
    ICheckersRulesetRepository
{
    public CheckersRulesetRepository(IRepositoryContext repositoryContext, bool initializeDefaultRulesets = false) :
        base(repositoryContext)
    {
        PreSaveActions.Add(cr => cr.UpdateLastModified());

        if (!initializeDefaultRulesets) return;

        var checkersRulesets = GetAllSaved();
        foreach (var checkersRuleset in DefaultCheckersRulesets.DefaultRulesets)
            if (!checkersRulesets.Any(other => other.IsEquivalent(checkersRuleset)))
                Add(checkersRuleset);
    }

    protected override string RepositoryName => "CheckersRuleset";

    public ICollection<CheckersRuleset> GetAllSaved()
    {
        return GetAll().Where(cr => cr.Saved).ToList();
    }
}