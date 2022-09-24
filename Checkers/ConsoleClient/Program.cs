// See https://aka.ms/new-console-template for more information

using MenuSystem;

Console.WriteLine("Hello, World!");

var n = new MenuItem("New game", () => Console.WriteLine("NEWGAME"));
var l = new MenuItem("Load game", () => Console.WriteLine("LOADGAME"));

var a = new MenuItem("Aadsfaasdfasdfasdfjhakdefcklasdkcfjasdhjfkjvsadhfjksadhfkjasdhfkjasdhfkjasdhfvkjsadfhsadkjdddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaddddddddddddddddddddddddddddddsdf", () => Console.WriteLine("ASDHGSJHDGASJ"));
var b = new MenuItem("Brgagaf", () => Console.WriteLine("BDFGJDHFHJKA"));

var extraMenuCreator = new ConsoleMenuFactory("Extra", n, l, a, b, n, l, l, l, a, l, n, b, l, a);
var extraItemCreator = new MenuItem("Extra", extraMenuCreator);
var optionsMenuCreator = new ConsoleMenuFactory("Options", a, b, l, extraItemCreator);
var optionsItemCreator = new MenuItem("Options", optionsMenuCreator);

var mainMenuCreator = new ConsoleMenuFactory("Main menu", n, l, optionsItemCreator);

var mainMenu = mainMenuCreator.Create();

mainMenu.Run();