using Domain.Model;

namespace DAL;

public interface ICheckersRulesetRepository : IRepository<CheckersRuleset>
{
    ICollection<CheckersRuleset> GetAllSaved();
}