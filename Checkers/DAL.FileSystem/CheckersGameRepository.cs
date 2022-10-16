using Domain;

namespace DAL.FileSystem;

public class CheckersGameRepository : AbstractFileSystemRepository<CheckersGame>, ICheckersGameRepository
{
    protected override string RepositoryName => "CheckersGame";

    public CheckersGameRepository(RepositoryContext repositoryContext) : base(repositoryContext)
    {
    }

    protected override CheckersGame Deserialize(string serializedData)
    {
        var result = base.Deserialize(serializedData);

        result.CheckersOptions = RepositoryContext.CheckersOptionsRepository.GetById(result.CheckersOptionsId);
        result.CheckersStates = RepositoryContext.CheckersStateRepository.GetByCheckersGameId(result.Id);

        return result;
    }

    protected override string Serialize(CheckersGame entity)
    {
        RepositoryContext.CheckersOptionsRepository.Upsert(entity.CheckersOptions);
        entity.CheckersOptionsId = entity.CheckersOptions.Id;

        foreach (var checkersState in entity.CheckersStates)
        {
            checkersState.CheckersGameId = entity.Id;
            RepositoryContext.CheckersStateRepository.Add(checkersState);
        }

        return base.Serialize(entity);
    }
}