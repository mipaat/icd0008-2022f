// See https://aka.ms/new-console-template for more information

using ConsoleClient;
using ConsoleMenuSystem;
using ConsoleUI;
using DAL.FileSystem;
using Domain;
using GameBrain;

var selectedCheckersOptions = new CheckersOptions();
CheckersGame? selectedCheckersGame = null;
var ctx = new RepositoryContext();

EMenuFunction RunConsoleGame(Menu menu)
{
    try
    {
        var consoleGame = selectedCheckersGame != null
            ? new ConsoleGame(menu.ConsoleWindow, new CheckersBrain(selectedCheckersGame), ctx)
            : new ConsoleGame(menu.ConsoleWindow, new CheckersBrain(selectedCheckersOptions), ctx);
        consoleGame.Run();
    }
    catch (ArgumentOutOfRangeException e)
    {
        menu.ConsoleWindow.MessageBox("An error occurred when attempting to run Checkers game!", e.Message);
    }
    finally
    {
        selectedCheckersOptions = null;
        selectedCheckersGame = null;
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

    selectedCheckersOptions = new CheckersOptions { Width = x, Height = y, Title = $"{x}x{y}", Saved = false };
    return RunConsoleGame(menu);
});

var loadGameOptionsMenuFactory = new MenuFactory("Pick a game options preset")
{
    MenuItemsFunc = () =>
    {
        var gameOptionsPresets = new List<MenuItem>();
        foreach (var checkersOptions in ctx.CheckersOptionsRepository.GetAll().Where(co => co.Saved))
        {
            gameOptionsPresets.Add(new MenuItem(checkersOptions.Title, m =>
            {
                selectedCheckersOptions = checkersOptions;
                return RunConsoleGame(m);
            }));
        }

        gameOptionsPresets.Add(customBoardSize);

        return gameOptionsPresets;
    }
};

var loadGameMenuFactory = new MenuFactory("Load game")
{
    MenuItemsFunc = () =>
    {
        var gamesToLoad = new List<MenuItem>();
        foreach (var checkersGame in ctx.CheckersGameRepository.GetAll())
        {
            gamesToLoad.Add(new MenuItem(
                $"{checkersGame.Id} | {checkersGame.CheckersOptions.Title} - Started: {checkersGame.StartedAt}, Last played: {checkersGame.CurrentCheckersState?.CreatedAt ?? checkersGame.StartedAt}",
                m =>
                {
                    selectedCheckersGame = checkersGame;
                    return RunConsoleGame(m);
                }));
        }

        return gamesToLoad;
    }
};

var deleteGameMenuFactory = new MenuFactory("Delete game")
{
    MenuItemsFunc = () =>
    {
        var gamesToDelete = new List<MenuItem>();
        foreach (var checkersGame in ctx.CheckersGameRepository.GetAll())
        {
            gamesToDelete.Add(new MenuItem(
                $"{checkersGame.Id} | {checkersGame.CheckersOptions.Title} - Started: {checkersGame.StartedAt}, Last played: {checkersGame.CurrentCheckersState?.CreatedAt ?? checkersGame.StartedAt}",
                m =>
                {
                    ctx.CheckersGameRepository.Remove(checkersGame.Id);
                    return EMenuFunction.MainMenu;
                }));
        }

        return gamesToDelete;
    }
};

var mainMenuCreator = new MenuFactory("Main menu",
    new MenuItem("New game", loadGameOptionsMenuFactory),
    new MenuItem("Load game", loadGameMenuFactory),
    new MenuItem("Delete game", deleteGameMenuFactory));

var window = new ConsoleWindow("Checkers", 50, 20);
var mainMenu = mainMenuCreator.Create(window);

mainMenu.Run();
window.Close();