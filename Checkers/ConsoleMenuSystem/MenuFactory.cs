using ConsoleUI;

namespace ConsoleMenuSystem;

public class MenuFactory
{
    private Func<Menu, List<MenuItem>> MenuItemsFunc { get; }
    public bool AppendDefaultMenuItems;
    public bool IsExitable = true;

    private Func<Menu, string> TitleFunc { get; }

    public MenuFactory(Func<Menu, string> titleFunc, Func<Menu, List<MenuItem>> menuItemsFunc,
        bool appendDefaultMenuItems = true)
    {
        TitleFunc = titleFunc;
        MenuItemsFunc = menuItemsFunc;
        AppendDefaultMenuItems = appendDefaultMenuItems;
    }

    public MenuFactory(string title, Func<Menu, List<MenuItem>> menuItemsFunc, bool appendDefaultMenuItems = true) :
        this(_ => title, menuItemsFunc, appendDefaultMenuItems)
    {
    }

    public MenuFactory(string title, params MenuItem[] menuItems) :
        this(title, _ => menuItems.ToList())
    {
    }

    public MenuFactory(Func<Menu, string> titleFunc, params MenuItem[] menuItems) :
        this(titleFunc, _ => menuItems.ToList())
    {
    }

    public Menu Create(ConsoleWindow consoleWindow, Menu? parentMenu = null)
    {
        return new Menu(consoleWindow, TitleFunc, MenuItemsFunc, parentMenu, AppendDefaultMenuItems)
        {
            IsExitable = IsExitable
        };
    }
}