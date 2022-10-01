// See https://aka.ms/new-console-template for more information

using ConsoleMenuSystem;

Console.WriteLine("Hello, World!");

var n = new MenuItem("New game", () => Console.WriteLine("NEWGAME"));
var l = new MenuItem("Load game", () => Console.WriteLine("LOADGAME"));

var a = new MenuItem("Aadsfaa", () => Console.WriteLine("ASDHGSJHDGASJ"));
var b = new MenuItem("Brgagaf", () => Console.WriteLine("BDFGJDHFHJKA"));

var extraMenuCreator = new MenuFactory("Extra", n, l, a, b, n, l, l, l, a, l, n, b, l, a);
var extraItemCreator = new MenuItem("Extra", extraMenuCreator);
var optionsMenuCreator = new MenuFactory("Options", a, b, l, extraItemCreator);
var optionsItemCreator = new MenuItem("Options", optionsMenuCreator);

var mainMenuCreator = new MenuFactory("Main menu", n, l, optionsItemCreator);

var window = new ConsoleWindow();
var mainMenu = mainMenuCreator.Create(window);

mainMenu.Run();