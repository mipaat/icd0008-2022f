using ConsoleUI;

namespace ConsoleMenuSystem;

public class Menu
{
    public static readonly MenuItem Exit = new("Exit", EMenuFunction.Exit);
    public static readonly MenuItem Back = new("Back", EMenuFunction.Back);
    public static readonly MenuItem MainMenu = new("Main Menu", EMenuFunction.MainMenu);
    private const int Height = 12;

    public readonly ConsoleWindow ConsoleWindow;
    public readonly Menu? ParentMenu;

    private readonly Func<Menu, string> _titleFunc;
    public string Title => _titleFunc(this);

    private readonly bool _appendDefaultMenuItems;

    private readonly Func<Menu, List<MenuItem>> _menuItemsFunc;
    private List<MenuItem>? _menuItemsCache;

    public bool IsExitable = true;

    public List<MenuItem> MenuItems
    {
        get
        {
            if (_menuItemsCache == null)
            {
                _menuItemsCache = _menuItemsFunc(this);
            }
            var result = _menuItemsCache.ToList();

            if (_appendDefaultMenuItems) result.AddRange(BuiltInMenuItems);

            return result;
        }
    }

    public List<MenuItem> BuiltInMenuItems
    {
        get
        {
            var result = new List<MenuItem>();
            if (ParentMenu is not null) result.Add(Back);
            if (Hierarchy.Count > 2) result.Add(MainMenu);
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
        get
        {
            _cursorPosition = Math.Max(Math.Min(_cursorPosition, MenuItems.Count - 1), 0);
            return _cursorPosition;
        }
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

    public Menu(ConsoleWindow consoleWindow,
        Func<Menu, string> titleFunc,
        Func<Menu, List<MenuItem>> menuItemsFunc,
        Menu? parentMenu = null,
        bool appendDefaultMenuItems = true)
    {
        ConsoleWindow = consoleWindow;
        _titleFunc = titleFunc;
        _menuItemsFunc = menuItemsFunc;
        ParentMenu = parentMenu;
        _appendDefaultMenuItems = appendDefaultMenuItems;
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

    private void WriteMenuItems(int start, int end)
    {
        var menuItems = MenuItems;
        for (var i = start; i < end; i++)
        {
            if (i < menuItems.Count)
            {
                var menuItem = menuItems[i];
                ConsoleWindow.AddLine(menuItem.Text, backgroundColor: i == CursorPosition ? ConsoleColor.White : null);
            }
            else
            {
                ConsoleWindow.AddLine();
            }
        }
    }

    private void ClearMenuItemsCache()
    {
        _menuItemsCache = null;
    }

    private int LongestLine()
    {
        return Math.Max(MenuItems.Aggregate(0, (i, item) => Math.Max(i, item.Text.Length)), Title.Length);
    }

    private EMenuFunction MenuLoop()
    {
        do
        {
            ConsoleWindow.AddLine(Title);

            var menuItemsHeight = Height - 2; // -2 for the surrounding separator lines

            var page = CursorPosition / menuItemsHeight;
            var menuItemsStart = page * menuItemsHeight;

            ConsoleWindow.AddLinePattern(page == 0 ? "_" : "▲", Math.Max(LongestLine(), 20));
            WriteMenuItems(menuItemsStart, menuItemsStart + menuItemsHeight);

            var maxPage = (MenuItems.Count - 1) / menuItemsHeight;
            ConsoleWindow.AddLinePattern(page < maxPage ? "▼" : "_", Math.Max(LongestLine(), 20));

            ConsoleWindow.Render();

            var input = ConsoleWindow.AwaitKeyInput();
            switch (input.KeyInfo?.Key)
            {
                case ConsoleKey.DownArrow:
                    IncrementCursorPosition();
                    break;
                case ConsoleKey.UpArrow:
                    DecrementCursorPosition();
                    break;
                case ConsoleKey.Escape:
                case ConsoleKey.Backspace:
                    if (IsExitable)
                    {
                        ClearMenuItemsCache();
                        return EMenuFunction.Continue;
                    }
                    break;
                case ConsoleKey.RightArrow:
                    IncrementCursorPosition(menuItemsHeight);
                    break;
                case ConsoleKey.LeftArrow:
                    DecrementCursorPosition(menuItemsHeight);
                    break;
                case ConsoleKey.Enter:
                    var menuItem = MenuItems[CursorPosition];
                    var menuFunction = menuItem.Run(this);
                    ClearMenuItemsCache();
                    switch (menuFunction)
                    {
                        case EMenuFunction.Back:
                            return EMenuFunction.Continue;
                        case EMenuFunction.Exit:
                            return menuFunction;
                        case EMenuFunction.MainMenu:
                            if (ParentMenu is not null) return menuFunction;
                            break;
                        case EMenuFunction.Refresh:
                            return menuFunction;
                    }

                    break;
            }
        } while (true);
    }

    public EMenuFunction Run()
    {
        
        var result = MenuLoop();
        ClearMenuItemsCache();
        return result;
    }

    public override string ToString()
    {
        return $"Menu({Title}, MenuItems: {MenuItems.Count})";
    }
}