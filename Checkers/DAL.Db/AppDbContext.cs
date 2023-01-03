using Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace DAL.Db;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<CheckersGame> CheckersGames { get; set; } = default!;
    public DbSet<CheckersRuleset> CheckersRulesets { get; set; } = default!;
    public DbSet<CheckersState> CheckersStates { get; set; } = default!;
}