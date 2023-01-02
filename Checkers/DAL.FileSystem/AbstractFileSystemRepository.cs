using Common;
using Domain;

namespace DAL.FileSystem;

public abstract class AbstractFileSystemRepository<T> : IRepository<T>
    where T : class, IDatabaseEntity, new()
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

    protected AbstractFileSystemRepository(IRepositoryContext repositoryContext)
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

    private void RunPreSaveActions(T entity)
    {
        foreach (var action in PreSaveActions)
        {
            action(entity);
        }
    }

    protected readonly IRepositoryContext RepositoryContext;

    public virtual ICollection<T> GetAll()
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

    public virtual T? GetById(int id)
    {
        EnsureDirectoryExists();
        if (!Exists(id)) return null;

        var filePath = GetFilePath(id);
        var fileContent = File.ReadAllText(filePath);
        var entity = Deserialize(fileContent);

        return entity;
    }

    public virtual void Add(T entity)
    {
        EnsureDirectoryExists();

        if (!entity.Id.Equals(0))
            throw new ArgumentException(
                $"Can't add entity {typeof(T).Name} with ID {entity.Id} - ID must be uninitialized when adding!");

        entity.Id = IncrementNextId();

        RunPreSaveActions(entity);

        var fileContent = Serialize(entity);
        File.WriteAllText(GetFilePath(entity.Id), fileContent);
    }

    public virtual void Update(T entity)
    {
        EnsureDirectoryExists();

        if (!Exists(entity.Id))
            throw new ArgumentException($"Can't update entity {typeof(T).Name} with ID {entity.Id} - ID not found!");
        RunPreSaveActions(entity);
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

    public T? Remove(int id)
    {
        var entity = GetById(id);
        return entity == null ? entity : Remove(entity);
    }

    public virtual T Remove(T entity)
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
        var fetchedEntity = GetById(entity.Id);
        if (fetchedEntity == null) throw new IllegalStateException($"Failed to refresh entity {entity} - fetched data was null!");
        entity.Refresh(fetchedEntity);
    }

    public Type EntityType => typeof(T);
}