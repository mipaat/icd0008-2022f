// See https://aka.ms/new-console-template for more information

using MenuSystem;

Console.WriteLine("Hello, World!");

var n = new MenuItem("New game", m =>
{
    Console.WriteLine("NEWGAME");
    return EMenuFunction.Continue;
});
var l = new MenuItem("Load game", m => EMenuFunction.Continue);

var a= new MenuItem("Aadsfasdf", m => EMenuFunction.Continue);
var b = new MenuItem("Brgagaf", m => EMenuFunction.Continue);

var extraMenuCreator = Menu.MenuCreator("Extra", n, l, a);
var extraItemCreator = new MenuItem("Extra", extraMenuCreator);
var optionsMenuCreator = Menu.MenuCreator("Options", a, b, l, extraItemCreator);
var optionsItemCreator = new MenuItem("Options", optionsMenuCreator);

var mainMenuCreator = Menu.MenuCreator("Main menu", n, l, optionsItemCreator);

var mainMenu = mainMenuCreator(null); 

mainMenu.Run();
