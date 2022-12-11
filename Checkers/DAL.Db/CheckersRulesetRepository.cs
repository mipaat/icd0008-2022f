using Domain;
using Microsoft.EntityFrameworkCore;

namespace DAL.Db;

public class CheckersRulesetRepository : AbstractDbRepository<CheckersRuleset>, ICheckersRulesetRepository
{
    public CheckersRulesetRepository(AppDbContext dbContext, IRepositoryContext repoContext) : base(dbContext, repoContext)
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

    protected override DbSet<CheckersRuleset> ThisDbSet => DbContext.CheckersRulesets;

    private bool ExistsEquivalent(CheckersRuleset checkersRuleset)
    {
        return ThisDbSet.AsEnumerable().Any(other => other.IsEquivalent(checkersRuleset));
    }

    public ICollection<CheckersRuleset> GetAllSaved()
    {
        return RunPreFetchActions(ThisDbSet.Where(cr => cr.Saved)).ToList();
    }
}