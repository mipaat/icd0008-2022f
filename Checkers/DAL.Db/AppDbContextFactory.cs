using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DAL.Db;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public static AppDbContext CreateDbContext()
    {
        var directorySeparator = Path.DirectorySeparatorChar;
        var sqliteRepoDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                                  directorySeparator + "ICD0008Checkers" +
                                  directorySeparator + "Data" +
                                  directorySeparator + "Sqlite" +
                                  directorySeparator;
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        Directory.CreateDirectory(sqliteRepoDirectory);
        optionsBuilder.UseSqlite("Data source=" + sqliteRepoDirectory + "Checkers.db");

        return new AppDbContext(optionsBuilder.Options);
    }

    public AppDbContext CreateDbContext(string[] args)
    {
        return CreateDbContext();
    }
}