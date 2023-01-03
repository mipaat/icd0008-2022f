using DAL;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.MyLibraries.PageModels;

public abstract class PageModelDb : PageModel
{
    protected readonly IRepositoryContext Ctx;

    protected PageModelDb(IRepositoryContext ctx)
    {
        Ctx = ctx;
    }
}