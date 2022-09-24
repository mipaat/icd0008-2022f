namespace MenuSystem;

public class ConsoleMenuFactory : MenuFactory
{
    private ConsoleMenuFactory(string title, Func<Menu, EMenuFunction> menuLoop, params MenuItem[] menuItems) : base(
        title, menuLoop, menuItems)
    {
    }

    public ConsoleMenuFactory(string title, params MenuItem[] menuItems) : this(title, _consoleMenuLoop, menuItems)
    {
    }

    private static Func<Menu, EMenuFunction> _consoleMenuLoop = menu =>
    {
        do
        {
            Console.WriteLine("\n");
            var menuPath = menu.MenuPath;
            if (menuPath.Length > 0) Console.WriteLine(menuPath);
            Console.WriteLine("========================");

            var menuItems = menu.MenuItems;

            for (var i = 0; i < menuItems.Count; i++)
            {
                var menuItem = menuItems[i];

                if (i == menu.CursorPosition)
                {
                    var previousBackgroundColor = Console.BackgroundColor;
                    Console.BackgroundColor = ConsoleColor.White;
                    Console.WriteLine(menuItem);
                    Console.BackgroundColor = previousBackgroundColor;
                }
                else
                {
                    Console.WriteLine(menuItem);
                }
            }

            var pressedKey = Console.ReadKey(true).Key;
            switch (pressedKey)
            {
                case ConsoleKey.DownArrow:
                    menu.IncrementCursorPosition();
                    break;
                case ConsoleKey.UpArrow:
                    menu.DecrementCursorPosition();
                    break;
                case ConsoleKey.Enter:
                    var menuItem = menuItems[menu.CursorPosition];
                    var menuFunction = menuItem.Run(menu);
                    switch (menuFunction)
                    {
                        case EMenuFunction.Back:
                            return EMenuFunction.Continue;
                        case EMenuFunction.Exit:
                            return menuFunction;
                        case EMenuFunction.MainMenu:
                            if (menu.ParentMenu is not null) return menuFunction;
                            break;
                    }

                    break;
            }
        } while (true);
    };
}