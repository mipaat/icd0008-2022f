namespace MenuSystem;

public class Menu
{
    public string Title { get; set; }

    private int _maxShortcutAttempts = 5;

    public Menu? ParentMenu = null;

    private static MenuItem _exit = new MenuItem("Exit", s => throw new ExitMenuException(), "x");
    private MenuItem _back = new MenuItem("Back", s => throw new BackMenuException(), "b");
    private List<MenuItem> _menuItems = new List<MenuItem>();

    public List<MenuItem> MenuItems()
    {
        var result = new List<MenuItem>(_menuItems);
        if (ParentMenu is not null) result.Add(_back);
        result.Add(_exit);
        return result;
    }

    public MenuItem? GetMenuItem(string shortcut)
    {
        foreach (var menuItem in MenuItems().Where(menuItem => menuItem.Shortcut.Equals(shortcut)))
        {
            return menuItem;
        }

        return null;
    }

    public void AddMenuItem(MenuItem menuItem)
    {
        var i = 0;
        var shortcut = menuItem.Shortcut;
        var usedShortcuts = UsedShortcuts();
        while (usedShortcuts.Contains(shortcut))
        {
            if (i >= _maxShortcutAttempts) throw new NoValidShortcutException(menuItem, i);
            shortcut = menuItem.Shortcut + i;
            i++;
        }

        menuItem.Shortcut = shortcut;
        _menuItems.Add(menuItem);
    }

    private HashSet<string> UsedShortcuts()
    {
        var result = new HashSet<string>();
        foreach (var menuItem in MenuItems())
        {
            result.Add(menuItem.Shortcut);
        }

        return result;
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
            foreach (var menuItem in MenuItems())
            {
                Console.WriteLine(menuItem);
            }

            var input = Console.ReadLine() ?? "";

            try
            {
                var menuItem = GetMenuItem(input.Trim().ToLower());
                if (menuItem is null) {continue;}
                menuItem.Run(input);
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
                }
            }
        } while (!exit);
    }
    
    public void RunMenu()
    {
        try
        {
            MenuLoop();
        }
        catch (ExitMenuException e)
        {
            if (ParentMenu is not null) throw;
        }
    }

    public string GetMenuPath()
    {
        return (ParentMenu?.GetMenuPath() ?? "") + Title + "/";
    }

    public static Func<Menu?, Menu> MenuCreator(string title, params Func<MenuItem>[] menuItemCreators)
    {
        return (Menu? parentMenu) =>
        {
            var resultMenu = new Menu(title, parentMenu);

            foreach (var menuItemCreator in menuItemCreators)
            {
                resultMenu.AddMenuItem(menuItemCreator());
            }

            return resultMenu;
        };
    }
}