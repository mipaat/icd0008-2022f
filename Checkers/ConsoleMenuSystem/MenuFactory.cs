using ConsoleUI;

namespace ConsoleMenuSystem;

public class MenuFactory
{
    private Func<Menu, List<MenuItem>> MenuItemsFunc { get; }
    public bool AppendDefaultMenuItems = true;
    public bool IsExitable = true;
    public string? CustomHeader = null;
    public Func<string?>? CustomHeaderFunc = null;
    public CustomMenuItemsCacheModifier? CustomBehaviour;

    private Func<Menu, string> TitleFunc { get; }

    public MenuFactory(Func<Menu, string> titleFunc, Func<Menu, List<MenuItem>> menuItemsFunc)
    {
        TitleFunc = titleFunc;
        MenuItemsFunc = menuItemsFunc;
    }

    public MenuFactory(string title, Func<Menu, List<MenuItem>> menuItemsFunc) :
        this(_ => title, menuItemsFunc)
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
        return new Menu(consoleWindow, TitleFunc, MenuItemsFunc, parentMenu)
        {
            IsExitable = IsExitable,
            AppendDefaultMenuItems = AppendDefaultMenuItems,
            GetCustomHeader = CustomHeaderFunc ?? (() => CustomHeader),
            CustomBehaviour = CustomBehaviour
        };
    }
}