using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DAL.Db;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        return CreateDbContext();
    }

    public static AppDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        ConfigureOptions(optionsBuilder);

        return new AppDbContext(optionsBuilder.Options);
    }

    public static void ConfigureOptions(DbContextOptionsBuilder optionsBuilder)
    {
        var directorySeparator = Path.DirectorySeparatorChar;
        var sqliteRepoDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                                  directorySeparator + "ICD0008Checkers" +
                                  directorySeparator + "Data" +
                                  directorySeparator + "Sqlite" +
                                  directorySeparator;

        Directory.CreateDirectory(sqliteRepoDirectory);
        optionsBuilder.UseSqlite("Data source=" + sqliteRepoDirectory + "Checkers.db");
    }
}