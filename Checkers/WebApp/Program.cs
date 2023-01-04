using DAL;
using DAL.Db;
using DAL.Filters;
using Microsoft.EntityFrameworkCore;
using WebApp.MyLibraries.ModelBinders;

var builder = WebApplication.CreateBuilder(args);

using (var ctx = AppDbContextFactory.CreateDbContext())
{
    ctx.Database.Migrate();
}

// Add services to the container.
builder.Services.AddRazorPages()
    .AddMvcOptions(options =>
    {
        options.ModelBinderProviders.Insert(0, new CustomModelBinderProvider<CompletionFilter>());
        options.ModelBinderProviders.Insert(0, new CustomModelBinderProvider<AiTypeFilter>());
        options.ModelBinderProviders.Insert(0, new CustomModelBinderProvider<SavedFilter>());
    });
// var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddDbContext<AppDbContext>(AppDbContextFactory.ConfigureOptions);
builder.Services.AddScoped<IRepositoryContext, RepositoryContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();