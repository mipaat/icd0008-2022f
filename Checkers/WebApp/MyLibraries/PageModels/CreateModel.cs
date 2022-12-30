using DAL;
using Domain;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.MyLibraries.PageModels;

public abstract class CreateModel<T> : RepositoryModel<T> where T : class, IDatabaseEntity, new()
{
    protected CreateModel(IRepositoryContext ctx) : base(ctx)
    {
    }

    [BindProperty] public T Entity { get; set; } = default!;

    public IActionResult OnGet()
    {
        return Page();
    }

    // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        Repository.Add(Entity);

        return RedirectToPage("./Index");
    }
}