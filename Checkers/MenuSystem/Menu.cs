namespace MenuSystem;

public class Menu
{
    public string Title { get; }

    public readonly Menu? ParentMenu;

    private static readonly MenuItem Exit = new("Exit", EMenuFunction.Exit);
    private static readonly MenuItem Back = new("Back", EMenuFunction.Back);
    private static readonly MenuItem Main = new("Main Menu", EMenuFunction.MainMenu);
    private readonly List<MenuItem> _menuItems = new();

    public List<MenuItem> MenuItems
    {
        get
        {
            var result = new List<MenuItem>(_menuItems);
            if (ParentMenu is not null) result.Add(Back);
            if (Hierarchy.Count > 2) result.Add(Main);
            result.Add(Exit);
            return result;
        }
    }

    public List<Menu> Hierarchy
    {
        get
        {
            var result = new List<Menu>();
            if (ParentMenu is not null)
            {
                result.AddRange(ParentMenu.Hierarchy);
            }

            result.Add(this);
            return result;
        }
    }

    private int _cursorPosition;

    public int CursorPosition
    {
        get => _cursorPosition;
        set
        {
            var menuItems = MenuItems;
            if (value < 0 || value >= menuItems.Count)
                throw new ArgumentOutOfRangeException(
                    $"Can't move cursor from {_cursorPosition} to {value} - out of Menu bounds ({0} - {menuItems.Count - 1})!");
            _cursorPosition = value;
        }
    }

    public string MenuPath => (ParentMenu?.MenuPath ?? "") + Title + "/";

    public Menu(string title, Menu? parentMenu = null, params MenuItem[] menuItems)
    {
        Title = title;
        ParentMenu = parentMenu;

        AddMenuItems(menuItems);
    }

    public int IncrementCursorPosition(int amount = 1)
    {
        var menuItems = MenuItems;
        if (amount < 0) amount = menuItems.Count + amount;
        amount %= menuItems.Count;
        CursorPosition = (CursorPosition + amount) % MenuItems.Count;
        return CursorPosition;
    }

    public int DecrementCursorPosition(int amount = 1)
    {
        return IncrementCursorPosition(-amount);
    }

    public void AddMenuItem(MenuItem menuItem)
    {
        _menuItems.Add(menuItem);
    }

    public void AddMenuItems(params MenuItem[] menuItems)
    {
        foreach (var menuItem in menuItems)
        {
            AddMenuItem(menuItem);
        }
    }

    public EMenuFunction Run()
    {
        do
        {
            Console.WriteLine("\n");
            var menuPath = MenuPath;
            if (menuPath.Length > 0) Console.WriteLine(menuPath);
            Console.WriteLine("========================");

            var menuItems = MenuItems;

            for (var i = 0; i < menuItems.Count; i++)
            {
                var menuItem = menuItems[i];

                if (i == CursorPosition)
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
                    IncrementCursorPosition();
                    break;
                case ConsoleKey.UpArrow:
                    DecrementCursorPosition();
                    break;
                case ConsoleKey.Enter:
                    var menuItem = menuItems[CursorPosition];
                    var menuFunction = menuItem.Run(this);
                    switch (menuFunction)
                    {
                        case EMenuFunction.Back:
                            return EMenuFunction.Continue;
                        case EMenuFunction.Exit:
                            return menuFunction;
                        case EMenuFunction.MainMenu:
                            if (ParentMenu is not null) return menuFunction;
                            break;
                    }

                    break;
            }
        } while (true);
    }

    public static Func<Menu?, Menu> MenuCreator(string title, params MenuItem[] menuItems)
    {
        return parentMenu =>
        {
            var resultMenu = new Menu(title, parentMenu);

            foreach (var menuItem in menuItems)
            {
                resultMenu.AddMenuItem(menuItem);
            }

            return resultMenu;
        };
    }

    public override string ToString()
    {
        return Title;
    }
}