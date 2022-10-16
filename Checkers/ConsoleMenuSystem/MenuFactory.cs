using ConsoleUI;

namespace ConsoleMenuSystem;

public class MenuFactory
{
    public readonly string Title;
    public readonly string? Id;
    public List<MenuItem> StaticMenuItems;
    public Func<List<MenuItem>>? MenuItemsFunc;
    public List<MenuItem> MenuItems => MenuItemsFunc != null ? MenuItemsFunc() : StaticMenuItems;

    public MenuFactory(string title, params MenuItem[] menuItems) : this(title, null, menuItems)
    {
    }

    public MenuFactory(string title, string? id, params MenuItem[] menuItems)
    {
        Title = title;
        Id = id;
        StaticMenuItems = menuItems.ToList();
    }

    public Menu Create(ConsoleWindow consoleWindow, Menu? parentMenu = null)
    {
        return new Menu(Title, consoleWindow, Id, parentMenu, MenuItems.ToArray());
    }
}