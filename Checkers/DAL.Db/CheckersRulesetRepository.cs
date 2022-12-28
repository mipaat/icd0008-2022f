using Domain;
using Microsoft.EntityFrameworkCore;

namespace DAL.Db;

public sealed class CheckersRulesetRepository : AbstractDbRepository<CheckersRuleset>, ICheckersRulesetRepository
{
    public CheckersRulesetRepository(AppDbContext dbContext, IRepositoryContext repoContext,
        bool initializeDefaultRulesets = false) : base(dbContext, repoContext)
    {
        PreSaveActions.Add(cr => cr.UpdateLastModified());

        if (!initializeDefaultRulesets) return;

        var checkersRulesets = GetAllSaved();
        foreach (var checkersRuleset in DefaultCheckersRulesets.DefaultRulesets)
        {
            if (!checkersRulesets.Any(other => other.IsEquivalent(checkersRuleset)))
            {
                Add(checkersRuleset);
            }
        }
    }

    protected override DbSet<CheckersRuleset> ThisDbSet => DbContext.CheckersRulesets;

    public ICollection<CheckersRuleset> GetAllSaved()
    {
        return RunPreFetchActions(ThisDbSet.Where(cr => cr.Saved)).ToList();
    }
}