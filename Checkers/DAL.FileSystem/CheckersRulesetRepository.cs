using Domain;

namespace DAL.FileSystem;

public class CheckersRulesetRepository : AbstractFileSystemRepository<CheckersRuleset>, ICheckersRulesetRepository
{
    protected override string RepositoryName => "CheckersRuleset";

    public CheckersRulesetRepository(IRepositoryContext repositoryContext) : base(repositoryContext)
    {
        foreach (var checkersRuleset in DefaultCheckersRulesets.DefaultRulesets)
        {
            if (!ExistsEquivalent(checkersRuleset))
            {
                Add(checkersRuleset);
            }
        }

        PreSaveActions.Add(cr => cr.UpdateLastModified());
    }

    private bool ExistsEquivalent(CheckersRuleset checkersRuleset)
    {
        return GetAll().ToList().Exists(other => other.IsEquivalent(checkersRuleset));
    }

    public ICollection<CheckersRuleset> GetAllSaved()
    {
        return GetAll().Where(cr => cr.Saved).ToList();
    }
}