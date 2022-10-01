namespace ConsoleMenuSystem;

public class MenuFactory
{
    private readonly string Title;
    private readonly MenuItem[] MenuItems;

    public MenuFactory(string title, params MenuItem[] menuItems)
    {
        Title = title;
        MenuItems = menuItems;
    }

    public Menu Create(ConsoleWindow consoleWindow, Menu? parentMenu = null)
    {
        return new Menu(Title, consoleWindow, parentMenu, MenuItems);
    }
}