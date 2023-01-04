using DAL;
using Domain;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.MyLibraries.PageModels;

public abstract class EditModel<T> : EntityModel<T> where T : class, IDatabaseEntity, new()
{
    protected EditModel(IRepositoryContext ctx) : base(ctx)
    {
    }

    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see https://aka.ms/RazorPagesCRUD.
    public virtual IActionResult OnPost()
    {
        if (!ModelState.IsValid) return Page();

        Repository.Upsert(Entity);
        Success = true;

        return RedirectToPage("./Index");
    }
}