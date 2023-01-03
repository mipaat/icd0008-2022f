// See https://aka.ms/new-console-template for more information

using ConsoleClient;
using ConsoleUI;
using DAL.Db;
using Microsoft.EntityFrameworkCore;

var window = new ConsoleWindow("Checkers");

using var ctx = AppDbContextFactory.CreateDbContext();
ctx.Database.Migrate();

var repoDb = new RepositoryContext(ctx);
var repoFs = new DAL.FileSystem.RepositoryContext();

var mainMenuCreator = new MainMenuCreator(repoDb, repoFs);
var mainMenu = mainMenuCreator.CreateMainMenu(window);

mainMenu.Run();
window.Close();