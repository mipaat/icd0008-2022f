using DAL;
using Domain;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.MyLibraries.PageModels;

public abstract class DeleteModel<T> : EntityModel<T> where T : class, IDatabaseEntity, new()
{
    protected DeleteModel(IRepositoryContext ctx) : base(ctx)
    {
    }

    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see https://aka.ms/RazorPagesCRUD.
    public IActionResult OnPost(int? id)
    {
        if (id == null) return NotFound();

        Repository.Remove(id.Value);

        return RedirectToPage("./Index");
    }
}