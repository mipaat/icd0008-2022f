// See https://aka.ms/new-console-template for more information

using MenuSystem;

Console.WriteLine("Hello, World!");

var n = MenuItem.MenuItemCreator("New game", s => Console.WriteLine("NEWGAME"));
var l = MenuItem.MenuItemCreator("Load game", s => Console.WriteLine("LOADGAME"));

var a = MenuItem.MenuItemCreator("Aadfgadf", s => Console.WriteLine("ASDDFGDA"));
var b = MenuItem.MenuItemCreator("Brgagaf", s => Console.WriteLine("BASDQWEF"));

var optionsMenuCreator = Menu.MenuCreator("Options", a, b, l);
var optionsItemCreator = MenuItem.MenuItemCreator("Options", optionsMenuCreator);

var mainMenuCreator = Menu.MenuCreator("Main menu", n, l, optionsItemCreator);

var mainMenu = mainMenuCreator(null); 

mainMenu.RunMenu();