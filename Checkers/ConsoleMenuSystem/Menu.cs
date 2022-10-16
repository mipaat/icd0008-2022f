using ConsoleUI;

namespace ConsoleMenuSystem;

public class Menu
{
    public string Title { get; }
    public string? Id { get; }

    private const int Height = 12;

    public readonly Menu? ParentMenu;

    private static readonly MenuItem Exit = new("Exit", EMenuFunction.Exit);
    private static readonly MenuItem Back = new("Back", EMenuFunction.Back);
    private static readonly MenuItem Main = new("Main Menu", EMenuFunction.MainMenu);
    private readonly List<MenuItem> _menuItems = new();

    public MenuItem SelectedItem => MenuItems[CursorPosition];

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

    public Menu(string title, ConsoleWindow consoleWindow, string? id = null, Menu? parentMenu = null,
        params MenuItem[] menuItems)
    {
        Title = title;
        Id = id;
        ConsoleWindow = consoleWindow;
        ParentMenu = parentMenu;

        AddMenuItems(menuItems);
    }

    public MenuItem? GetSelectedItem(string menuId)
    {
        if (Id == menuId && _menuItems.Contains(SelectedItem))
        {
            return SelectedItem;
        }

        return ParentMenu?.GetSelectedItem(menuId);
    }

    public string? GetSelectedItemId(string menuId)
    {
        return GetSelectedItem(menuId)?.Id;
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
                ConsoleWindow.AddLine(menuItem.Text, i == CursorPosition ? ConsoleColor.White : null);
            }
            else
            {
                ConsoleWindow.AddLine();
            }
        }
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

            var page = menuItemsHeight != 0 ? CursorPosition / menuItemsHeight : 0;
            var menuItemsStart = page * menuItemsHeight;

            ConsoleWindow.AddLinePattern(page == 0 ? "_" : "▲", Math.Max(LongestLine(), 20));
            WriteMenuItems(menuItemsStart, menuItemsStart + menuItemsHeight);

            var maxPage = menuItemsHeight != 0 ? (MenuItems.Count - 1) / menuItemsHeight : page;
            ConsoleWindow.AddLinePattern(page < maxPage ? "▼" : "_", Math.Max(LongestLine(), 20));

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
                        case EMenuFunction.Refresh:
                            return menuFunction;
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