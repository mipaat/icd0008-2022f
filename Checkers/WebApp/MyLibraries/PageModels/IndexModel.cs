using DAL;
using DAL.Filters;
using Domain;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.MyLibraries.PageModels;

public abstract class IndexModel<T> : RepositoryModel<T> where T : class, IDatabaseEntity, new()
{
    protected IndexModel(IRepositoryContext ctx) : base(ctx)
    {
    }

    public IList<T> Entities { get; set; } = default!;

    protected virtual IEnumerable<FilterFunc<T>>? Filters { get; }

    public virtual IActionResult OnGet()
    {
        Entities = (Filters != null ? Repository.GetAll(Filters.ToArray()) : Repository.GetAll())
            .ToList();
        return Page();
    }
}