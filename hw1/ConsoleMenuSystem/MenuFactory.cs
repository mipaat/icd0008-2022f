using ConsoleUI;

namespace ConsoleMenuSystem;

public class MenuFactory
{
    public readonly string Title;
    public readonly string? Id;
    public readonly MenuItem[] MenuItems;

    public MenuFactory(string title, params MenuItem[] menuItems) : this(title, null, menuItems)
    {
    }

    public MenuFactory(string title, string? id, params MenuItem[] menuItems)
    {
        Title = title;
        Id = id;
        MenuItems = menuItems;
    }

    public Menu Create(ConsoleWindow consoleWindow, Menu? parentMenu = null)
    {
        return new Menu(Title, consoleWindow, Id, parentMenu, MenuItems);
    }
}