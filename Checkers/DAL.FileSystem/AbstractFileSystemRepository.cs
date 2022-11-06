using Domain;

namespace DAL.FileSystem;

public abstract class AbstractFileSystemRepository<T> : IRepository<T>
    where T : class, IDatabaseEntity
{
    private const string FileExtension = ".json";
    protected abstract string RepositoryName { get; }
    private static char DirectorySeparator => Path.DirectorySeparatorChar;

    private string RepositoryPath => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                                     DirectorySeparator + "ICD0008Checkers" +
                                     DirectorySeparator + "Data" +
                                     DirectorySeparator + "FileSystemRepo" +
                                     DirectorySeparator + RepositoryName;

    private string IdFilePath => RepositoryPath + DirectorySeparator + ".NextID";

    protected ICollection<Action<T>> PreSaveActions { get; }

    protected AbstractFileSystemRepository(RepositoryContext repositoryContext)
    {
        RepositoryContext = repositoryContext;
        PreSaveActions = new List<Action<T>>();
    }

    private void EnsureDirectoryExists()
    {
        Directory.CreateDirectory(RepositoryPath);
    }

    private string GetFilePath(int id)
    {
        return RepositoryPath + DirectorySeparator + id + FileExtension;
    }

    protected virtual T Deserialize(string serializedData)
    {
        var result = System.Text.Json.JsonSerializer.Deserialize<T>(serializedData);
        if (result == null)
            throw new NullReferenceException($"Could not deserialize {typeof(T).Name} from '{serializedData}'!");
        return result;
    }

    protected virtual string Serialize(T entity)
    {
        return System.Text.Json.JsonSerializer.Serialize(entity);
    }

    private int GetNextId()
    {
        const int attemptLimit = 5;
        for (var i = 0; i < attemptLimit; i++)
        {
            if (File.Exists(IdFilePath) && int.TryParse(File.ReadAllText(IdFilePath), out var id)) return id;

            File.WriteAllText(IdFilePath, "1");
        }

        throw new IOException($"Failed to get next Id in {attemptLimit} attempts!");
    }

    private int IncrementNextId()
    {
        var id = GetNextId();

        File.WriteAllText(IdFilePath, (id + 1).ToString());

        return id;
    }

    protected readonly RepositoryContext RepositoryContext;

    public ICollection<T> GetAll()
    {
        EnsureDirectoryExists();

        var result = new List<T>();

        foreach (var filePath in Directory.GetFiles(RepositoryPath, "*.json"))
        {
            var fileContent = File.ReadAllText(filePath);
            var entity = Deserialize(fileContent);
            result.Add(entity);
        }

        result.Sort((o1, o2) => o1.Id - o2.Id);
        return result;
    }

    public T GetById(int id)
    {
        EnsureDirectoryExists();
        if (!Exists(id)) throw new InvalidOperationException($"No {typeof(T).Name} with ID {id} found!");

        var filePath = GetFilePath(id);
        var fileContent = File.ReadAllText(filePath);
        var entity = Deserialize(fileContent);

        return entity;
    }

    public void Add(T entity)
    {
        EnsureDirectoryExists();

        if (!entity.Id.Equals(0))
            throw new ArgumentException(
                $"Can't add entity {typeof(T).Name} with ID {entity.Id} - ID must be uninitialized when adding!");

        entity.Id = IncrementNextId();

        var fileContent = Serialize(entity);
        File.WriteAllText(GetFilePath(entity.Id), fileContent);
    }

    public void Update(T entity)
    {
        EnsureDirectoryExists();

        if (!Exists(entity.Id))
            throw new ArgumentException($"Can't update entity {typeof(T).Name} with ID {entity.Id} - ID not found!");
        var fileContent = Serialize(entity);
        File.WriteAllText(GetFilePath(entity.Id), fileContent);
    }

    public void Upsert(T entity)
    {
        EnsureDirectoryExists();

        if (Exists(entity.Id))
        {
            Update(entity);
        }
        else
        {
            Add(entity);
        }
    }

    public T Remove(int id)
    {
        return Remove(GetById(id));
    }

    public T Remove(T entity)
    {
        File.Delete(GetFilePath(entity.Id));
        entity.Id = 0;
        return entity;
    }

    public bool Exists(int id)
    {
        return File.Exists(GetFilePath(id));
    }

    public void Refresh(T entity)
    {
        entity = Exists(entity.Id) ? GetById(entity.Id) : entity;
    }
}