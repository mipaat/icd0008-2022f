// See https://aka.ms/new-console-template for more information

using MenuSystem;

Console.WriteLine("Hello, World!");

var n = MenuItem.MenuItemCreator("New game", () => Console.WriteLine("NEWGAME"));
var l = MenuItem.MenuItemCreator("Load game", () => Console.WriteLine("LOADGAME"));

var a = MenuItem.MenuItemCreator("Aadfgadf", () => Console.WriteLine("ASDDFGDA"));
var b = MenuItem.MenuItemCreator("Brgagaf", () => Console.WriteLine("BASDQWEF"));

var extraMenuCreator = Menu.MenuCreator("Extra", n, l, a);
var extraItemCreator = MenuItem.MenuItemCreator("Extra", extraMenuCreator);
var optionsMenuCreator = Menu.MenuCreator("Options", a, b, l, extraItemCreator);
var optionsItemCreator = MenuItem.MenuItemCreator("Options", optionsMenuCreator);

var mainMenuCreator = Menu.MenuCreator("Main menu", n, l, optionsItemCreator);

var mainMenu = mainMenuCreator(null); 

mainMenu.RunMenu();