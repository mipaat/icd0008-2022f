using DAL;
using Domain;

namespace WebApp.MyLibraries.PageModels;

public abstract class RepositoryModel<T> : PageModelDb where T : class, IDatabaseEntity, new()
{
    protected RepositoryModel(IRepositoryContext ctx) : base(ctx)
    {
    }

    protected abstract IRepository<T> Repository { get; }
}