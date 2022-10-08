// See https://aka.ms/new-console-template for more information

using ConsoleClient;
using ConsoleMenuSystem;
using ConsoleUI;
using Game;

Func<Menu, EMenuFunction> notImplemented = menu =>
{
    menu.ConsoleWindow.MessageBox("Not implemented!");
    return EMenuFunction.Continue;
};

EMenuFunction RunConsoleGame(int x, int y, Menu menu)
{
    var gameOptions = new GameOptions
    {
        Width = x,
        Height = y
    };
    try
    {
        var consoleGame = new ConsoleGame(menu.ConsoleWindow, new GameBrain(gameOptions));
        consoleGame.Run();
    }
    catch (ArgumentOutOfRangeException e)
    {
        menu.ConsoleWindow.MessageBox("An error occurred when attempting to run Checkers game!", e.Message);
    }
    
    return EMenuFunction.MainMenu;
}

var eightByEight = new MenuItem("8x8", menu => RunConsoleGame(8, 8, menu));
var tenByTen = new MenuItem("10x10", menu => RunConsoleGame(10, 10, menu));
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

    return RunConsoleGame(x, y, menu);
});

var boardSizeMenuFactory = new MenuFactory("Board size", eightByEight, tenByTen, customBoardSize);

var versusAi = new MenuItem("VS AI", boardSizeMenuFactory, OpponentType.Ai.ToString());
var versusPlayer = new MenuItem("VS Player", boardSizeMenuFactory, OpponentType.Player.ToString());
var opponentMenuFactory = new MenuFactory("Opponent", nameof(OpponentType), versusAi, versusPlayer);

var local = new MenuItem("Local", opponentMenuFactory, GameType.Local.ToString());
// var online = new MenuItem("Online", opponentMenuFactory, GameType.Online.ToString());
var online = new MenuItem("Online (Not implemented)", notImplemented);
var newGameMenuFactory = new MenuFactory("Game type", nameof(GameType), local, online);

var n = new MenuItem("New game", newGameMenuFactory);

var mainMenuCreator = new MenuFactory("Main menu", n);

var window = new ConsoleWindow("Checkers", 50, 20);
var mainMenu = mainMenuCreator.Create(window);

mainMenu.Run();
window.Close();