using DAL.Filters;
using Domain.Model;

namespace DAL;

public interface ICheckersGameRepository : IRepository<CheckersGame>
{
    public CheckersGame? GetById(int id, bool includeAllCheckersStates = false);

    public ICollection<CheckersGame> GetAll(bool includeAllCheckersStates = false,
        params FilterFunc<CheckersGame>[] filters);
}