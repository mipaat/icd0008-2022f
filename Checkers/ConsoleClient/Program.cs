// See https://aka.ms/new-console-template for more information

using MenuSystem;

Console.WriteLine("Hello, World!");

var n = new MenuItem("New game", () => Console.WriteLine("NEWGAME"));
var l = new MenuItem("Load game", () => Console.WriteLine("LOADGAME"));

var a = new MenuItem("Aadsfasdf", () => Console.WriteLine("ASDHGSJHDGASJ"));
var b = new MenuItem("Brgagaf", () => Console.WriteLine("BDFGJDHFHJKA"));

var extraMenuCreator = Menu.MenuCreator("Extra", n, l, a);
var extraItemCreator = new MenuItem("Extra", extraMenuCreator);
var optionsMenuCreator = Menu.MenuCreator("Options", a, b, l, extraItemCreator);
var optionsItemCreator = new MenuItem("Options", optionsMenuCreator);

var mainMenuCreator = Menu.MenuCreator("Main menu", n, l, optionsItemCreator);

var mainMenu = mainMenuCreator(null);

mainMenu.Run();