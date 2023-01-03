using ConsoleUI;

namespace ConsoleMenuSystem;

public class MenuFactory
{
    public bool AppendDefaultMenuItems = true;
    public CustomMenuItemsCacheModifier? CustomBehaviour;
    public string? CustomHeader = null;
    public Func<string?>? CustomHeaderFunc = null;
    public bool IsExitable = true;

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

    private Func<Menu, List<MenuItem>> MenuItemsFunc { get; }

    private Func<Menu, string> TitleFunc { get; }

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