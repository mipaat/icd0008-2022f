// See https://aka.ms/new-console-template for more information

using ConsoleMenuSystem;

Console.WriteLine("Hello, World!");

Action dummyAction = () =>
{
    var _ = 1;
};

var eightByEight = new MenuItem("8x8", dummyAction);
var tenByTen = new MenuItem("10x10", dummyAction);
var customBoardSize = new MenuItem("Custom board size", menu =>
{
    var consoleWindow = menu.ConsoleWindow;

    if (!int.TryParse(consoleWindow.PromptTextInput("Enter board horizontal dimension:"), out var x))
    {
        consoleWindow.MessageBox("Board dimension must be an integer!");
        return EMenuFunction.Continue;
    }

    if (!int.TryParse(consoleWindow.PromptTextInput("Enter board vertical dimension:"), out var y))
    {
        consoleWindow.MessageBox("Board dimension must be an integer!");
        return EMenuFunction.Continue;
    }

    return EMenuFunction.Continue;
});
var newGameMenuCreator = new MenuFactory("New game", eightByEight, tenByTen, customBoardSize);
var n = new MenuItem("New game", newGameMenuCreator);

// var l = new MenuItem("Load game", dummyAction);
//
// var a = new MenuItem("Aadsfaa", dummyAction);
// var b = new MenuItem("Brgagaf", dummyAction);
//
// var extraMenuCreator = new MenuFactory("Extra", n, l, a, b, n, l, l, l, a, l, n, b, l, a);
// var extraItemCreator = new MenuItem("Extra", extraMenuCreator);
// var optionsMenuCreator = new MenuFactory("Options", a, b, l, extraItemCreator);
// var optionsItemCreator = new MenuItem("Options", optionsMenuCreator);

// var mainMenuCreator = new MenuFactory("Main menu", n, l, optionsItemCreator);

var mainMenuCreator = new MenuFactory("Main menu", n);

var window = new ConsoleWindow();
var mainMenu = mainMenuCreator.Create(window);

mainMenu.Run();