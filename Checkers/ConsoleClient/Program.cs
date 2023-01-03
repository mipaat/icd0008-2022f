// See https://aka.ms/new-console-template for more information

using ConsoleClient;
using ConsoleUI;
using Microsoft.EntityFrameworkCore;

var window = new ConsoleWindow("Checkers");

using (var ctx = DAL.Db.AppDbContextFactory.CreateDbContext())
{
    ctx.Database.Migrate();
}

var mainMenuCreator = new MainMenuCreator(
    new DAL.Db.RepositoryContextFactory(),
    new DAL.FileSystem.RepositoryContextFactory()
);
var mainMenu = mainMenuCreator.CreateMainMenu(window);

mainMenu.Run();
window.Close();