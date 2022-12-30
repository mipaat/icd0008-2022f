﻿using DAL;
using Domain;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.MyLibraries.PageModels;

public abstract class IndexModel<T> : RepositoryModel<T> where T : class, IDatabaseEntity, new()
{
    public IList<T> Entities { get; set; } = default!;
    
    public virtual IActionResult OnGet()
    {
        Entities = Repository.GetAll().ToList();
        return Page();
    }

    protected IndexModel(IRepositoryContext ctx) : base(ctx)
    {
    }
}