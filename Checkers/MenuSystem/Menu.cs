namespace MenuSystem;

public class Menu
{
    public string Title { get; set; }

    public Menu? ParentMenu = null;

    private static MenuItem _exit = new MenuItem("Exit", () => throw new ExitMenuException());
    private MenuItem _back = new MenuItem("Back", () => throw new BackMenuException());
    private MenuItem _main = new MenuItem("Main Menu", () => throw new MainMenuException());
    private List<MenuItem> _menuItems = new List<MenuItem>();
    public List<MenuItem> MenuItems {
        get
        {
            var result = new List<MenuItem>(_menuItems);
            if (ParentMenu is not null) result.Add(_back);
            if (GetHierarchy().Count > 2) result.Add(_main);
            result.Add(_exit);
            return result;
        }
        set => _menuItems = value;
    }

    private int _cursorPosition;
    public int CursorPosition
    {
        get => _cursorPosition;
        set
        {
            var menuItems = MenuItems;
            if (value < 0 || value >= menuItems.Count) throw new ArgumentOutOfRangeException(
                $"Can't move cursor from {_cursorPosition} to {value} - out of Menu bounds ({0} - {menuItems.Count - 1})!");
            _cursorPosition = value;
        }
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

    public Menu(string title, Menu? parentMenu = null)
    {
        Title = title;
        ParentMenu = parentMenu;

    }

    private void MenuLoop()
    {
        var exit = false;
        do
        {
            Console.WriteLine("\n");
            var menuPath = GetMenuPath();
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
                    try
                    {
                        var menuItem = menuItems[CursorPosition];
                        menuItem.Run();
                    }
                    catch (Exception e)
                    {
                        switch (e)
                        {
                            case BackMenuException:
                                exit = true;
                                break;
                            case SelectMenuException selectMenuException:
                                selectMenuException.Run(this);
                                break;
                            case ExitMenuException:
                                throw;
                            case MainMenuException:
                                throw;
                            default:
                                throw;
                        }
                    }

                    break;
            }


        } while (!exit);
    }
    
    public void RunMenu()
    {
        try
        {
            MenuLoop();
        }
        catch (ExitMenuException)
        {
            if (ParentMenu is not null) throw;
        }
        catch (MainMenuException)
        {
            if (ParentMenu is not null) throw;
            RunMenu();
        }
    }

    public List<Menu> GetHierarchy()
    {
        var result = new List<Menu>();
        if (ParentMenu is not null)
        {
            result.AddRange(ParentMenu.GetHierarchy());
        }
        result.Add(this);
        return result;
    }

    public string GetMenuPath()
    {
        return (ParentMenu?.GetMenuPath() ?? "") + Title + "/";
    }

    public static Func<Menu?, Menu> MenuCreator(string title, params Func<MenuItem>[] menuItemCreators)
    {
        return parentMenu =>
        {
            var resultMenu = new Menu(title, parentMenu);

            foreach (var menuItemCreator in menuItemCreators)
            {
                resultMenu.AddMenuItem(menuItemCreator());
            }

            return resultMenu;
        };
    }

    public override string ToString()
    {
        return Title;
    }
}