using Domain;

namespace DAL;

public interface ICheckersRulesetRepository : IRepository<CheckersRuleset>
{
    ICollection<CheckersRuleset> GetAllSaved();
}