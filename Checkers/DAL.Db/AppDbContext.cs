using Domain;
using Microsoft.EntityFrameworkCore;

namespace DAL.Db;

public class AppDbContext : DbContext
{
    public DbSet<CheckersGame> CheckersGames { get; set; } = default!;
    public DbSet<CheckersOptions> CheckersOptions { get; set; } = default!;
    public DbSet<CheckersState> CheckersStates { get; set; } = default!;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
}