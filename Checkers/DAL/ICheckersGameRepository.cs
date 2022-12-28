using Domain;

namespace DAL;

public interface ICheckersGameRepository : IRepository<CheckersGame>
{
    public CheckersGame? GetById(int id, bool noTracking = false, bool includeAllCheckersStates = false);
    public ICollection<CheckersGame> GetAll(bool includeAllCheckersStates = false);
}