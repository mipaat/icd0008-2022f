// See https://aka.ms/new-console-template for more information

using ConsoleClient;
using ConsoleMenuSystem;
using ConsoleUI;
using DAL.FileSystem;
using Domain;
using GameBrain;

// Func<Menu, EMenuFunction> notImplemented = menu =>
// {
//     menu.ConsoleWindow.MessageBox("Not implemented!");
//     return EMenuFunction.Continue;
// };

var selectedCheckersOptions = new CheckersOptions();

EMenuFunction RunConsoleGame(Menu menu)
{
    try
    {
        var consoleGame = new ConsoleGame(menu.ConsoleWindow, new CheckersBrain(selectedCheckersOptions));
        consoleGame.Run();
    }
    catch (ArgumentOutOfRangeException e)
    {
        menu.ConsoleWindow.MessageBox("An error occurred when attempting to run Checkers game!", e.Message);
    }

    return EMenuFunction.MainMenu;
}

var customBoardSize = new MenuItem("Custom board size", menu =>
{
    var consoleWindow = menu.ConsoleWindow;

    if (!int.TryParse(consoleWindow.PopupPromptTextInput("Enter board horizontal dimension:"), out var x))
    {
        consoleWindow.MessageBox("Board dimension must be an integer!");
        return EMenuFunction.Continue;
    }

    if (!int.TryParse(consoleWindow.PopupPromptTextInput("Enter board vertical dimension:"), out var y))
    {
        consoleWindow.MessageBox("Board dimension must be an integer!");
        return EMenuFunction.Continue;
    }

    selectedCheckersOptions.Width = x;
    selectedCheckersOptions.Height = y;
    return RunConsoleGame(menu);
});

var ctx = new RepositoryContext();
var loadGameOptionsMenuFactory = new MenuFactory("Pick a game options preset");
foreach (var checkersOptions in ctx.CheckersOptionsRepository.GetAll())
{
    loadGameOptionsMenuFactory.MenuItems.Add(new MenuItem(checkersOptions.Title, m =>
    {
        selectedCheckersOptions = checkersOptions;
        return RunConsoleGame(m);
    }));
}
loadGameOptionsMenuFactory.MenuItems.Add(customBoardSize);

var mainMenuCreator = new MenuFactory("Main menu", new MenuItem("New game", loadGameOptionsMenuFactory));

var window = new ConsoleWindow("Checkers", 50, 20);
var mainMenu = mainMenuCreator.Create(window);

mainMenu.Run();
window.Close();