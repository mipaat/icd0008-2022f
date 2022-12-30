using DAL;
using Domain;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.MyLibraries.PageModels;

public abstract class EntityModel<T> : RepositoryModel<T> where T : class, IDatabaseEntity, new()
{
    [BindProperty] public T Entity { get; set; } = default!;

    public IActionResult OnGet(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var entity = Repository.GetById(id.Value);
        if (entity == null)
        {
            return NotFound();
        }

        Entity = entity;
        return Page();
    }

    protected EntityModel(IRepositoryContext ctx) : base(ctx)
    {
    }
}