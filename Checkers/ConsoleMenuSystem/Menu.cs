namespace ConsoleMenuSystem;

public class Menu
{
    public string Title { get; }

    public readonly Menu? ParentMenu;

    private static readonly MenuItem Exit = new("Exit", EMenuFunction.Exit);
    private static readonly MenuItem Back = new("Back", EMenuFunction.Back);
    private static readonly MenuItem Main = new("Main Menu", EMenuFunction.MainMenu);
    private readonly List<MenuItem> _menuItems = new();
    private readonly List<Action<Menu>> _closingCallbacks;

    public readonly ConsoleWindow ConsoleWindow;

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

    public Menu(string title, ConsoleWindow consoleWindow, Menu? parentMenu = null, params MenuItem[] menuItems)
    {
        Title = title;
        ConsoleWindow = consoleWindow;
        ParentMenu = parentMenu;
        _closingCallbacks = new List<Action<Menu>>();

        AddMenuItems(menuItems);
    }

    public void AddClosingCallback(Action<Menu> callback)
    {
        _closingCallbacks.Add(callback);
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

    private void AddMenuItem(MenuItem menuItem)
    {
        _menuItems.Add(menuItem);
    }

    private void AddMenuItems(params MenuItem[] menuItems)
    {
        foreach (var menuItem in menuItems)
        {
            AddMenuItem(menuItem);
        }
    }

    private void WriteMenuItems(int start, int end)
    {
        var menuItems = MenuItems;
        for (var i = start; i < end; i++)
        {
            if (i < menuItems.Count)
            {
                var menuItem = menuItems[i];
                ConsoleWindow.AddLine(menuItem.Text, i == CursorPosition);
            }
            else
            {
                ConsoleWindow.AddLine();
            }
        }
    }
    
    private EMenuFunction MenuLoop()
    {
        Console.CursorVisible = false;
        do
        {
            ConsoleWindow.AddLine(MenuPath.Length > 0 ? MenuPath : "MENU PATH NOT FOUND???",
                conformLength: true, preferRight: true);
            
            var menuItemsHeight = ConsoleWindow.LinesLeft() - 2; // -2 for the surrounding separator lines

            var page = CursorPosition / menuItemsHeight;
            var menuItemsStart = page * menuItemsHeight;

            ConsoleWindow.AddLine(page == 0 ? '_' : '▲');
            WriteMenuItems(menuItemsStart, menuItemsStart + menuItemsHeight);

            var maxPage = (MenuItems.Count - 1) / menuItemsHeight;
            ConsoleWindow.AddLine(page < maxPage ? '▼' : '_');
            
            ConsoleWindow.Render();

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
                    var menuItem = MenuItems[CursorPosition];
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

    public EMenuFunction Run()
    {
        return MenuLoop();
    }

    public override string ToString()
    {
        return $"Menu({Title}, MenuItems: {MenuItems.Count})";
    }
}