using Domain.Model;

namespace DAL;

public interface ICheckersStateRepository : IRepository<CheckersState>
{
    public ICollection<CheckersState> GetByCheckersGameId(int checkersGameId);
}