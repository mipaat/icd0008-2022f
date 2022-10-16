using Domain;

namespace DAL.FileSystem;

public class CheckersStateRepository : AbstractFileSystemRepository<CheckersState>, ICheckersStateRepository
{
    public CheckersStateRepository(RepositoryContext repositoryContext) : base(repositoryContext)
    {
    }

    protected override string RepositoryName => "CheckersState";

    protected override CheckersState Deserialize(string serializedData)
    {
        var result = base.Deserialize(serializedData);

        result.DeserializeGamePieces();

        return result;
    }

    protected override string Serialize(CheckersState entity)
    {
        entity.SerializeGamePieces();

        return base.Serialize(entity);
    }

    public ICollection<CheckersState> GetByCheckersGameId(int checkersGameId)
    {
        return GetAll().Where(cs => cs.CheckersGameId.Equals(checkersGameId)).ToList();
    }
}