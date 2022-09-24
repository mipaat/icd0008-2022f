namespace MenuSystem;

public class ConsoleMenuFactory : MenuFactory
{
    public const int MaxMenuHeight = 10;
    public const int LineSeparatorWidth = 32;

    private ConsoleMenuFactory(string title, Func<Menu, EMenuFunction> menuLoop, params MenuItem[] menuItems) : base(
        title, menuLoop, menuItems)
    {
    }

    public ConsoleMenuFactory(string title, params MenuItem[] menuItems) : this(title, _consoleMenuLoop, menuItems)
    {
    }

    private static int WriteMenuHeader(Menu menu)
    {
        var writtenLines = 0;
        var menuPath = menu.MenuPath;
        if (menuPath.Length == 0)
        {
            menuPath = "MENU PATH NOT FOUND???";
        }

        OverWriteLine();
        writtenLines++;
        OverWriteLine(menuPath);
        writtenLines++;
        return writtenLines;
    }

    private static void OverWriteLine(string text = "", bool highlight = false)
    {
        var previousBackgroundColor = Console.BackgroundColor;
        if (highlight) Console.BackgroundColor = ConsoleColor.White;
        var processedText = text.Length > Console.WindowWidth ? text[..(Console.WindowWidth - 3)] + "..." : text;
        Console.Write(processedText);
        Console.BackgroundColor = previousBackgroundColor;
        Console.Write(new string(' ', Console.WindowWidth - processedText.Length));
    }

    private static void OverWriteLine(char pattern, int amount = LineSeparatorWidth)
    {
        OverWriteLine(new string(pattern, amount));
    }

    private static Func<Menu, EMenuFunction> _consoleMenuLoop = menu =>
    {
        Console.CursorVisible = false; // TODO: Make visible again when done
        var initialCursorTop = Console.CursorTop;
        do
        {
            var headerHeight = WriteMenuHeader(menu);
            var menuItemsHeight = MaxMenuHeight - headerHeight - 2;  // -2 for the surrounding separator lines

            var page = menu.CursorPosition / menuItemsHeight;
            var menuItemsStart = page * menuItemsHeight;

            OverWriteLine(page == 0 ? '_' : '▲');
            WriteMenuItems(menu, menuItemsStart, menuItemsStart + menuItemsHeight);

            var maxPage = (menu.MenuItems.Count - 1) / menuItemsHeight;
            OverWriteLine(page < maxPage ? '▼' : '_');

            Console.SetCursorPosition(0, initialCursorTop);

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
                    var menuItem = menu.MenuItems[menu.CursorPosition];
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
            
            Console.SetCursorPosition(0, initialCursorTop);
        } while (true);
    };

    private static void WriteMenuItems(Menu menu, int start, int end)
    {
        var menuItems = menu.MenuItems;
        for (var i = start; i < end; i++)
        {
            if (i < menuItems.Count)
            {
                var menuItem = menuItems[i];
                OverWriteLine(menuItem.Text, i == menu.CursorPosition);
            }
            else
            {
                OverWriteLine();
            }
        }
    }
}