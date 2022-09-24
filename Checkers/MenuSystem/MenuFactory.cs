namespace MenuSystem;

public class MenuFactory
{
    private readonly string _title;
    private readonly MenuItem[] _menuItems;
    private readonly Func<Menu, EMenuFunction> _menuLoop;

    public MenuFactory(string title, Func<Menu, EMenuFunction> menuLoop, params MenuItem[] menuItems)
    {
        _title = title;
        _menuLoop = menuLoop;
        _menuItems = menuItems;
    }

    public Menu Create(Menu? parentMenu = null)
    {
        return new Menu(_title, _menuLoop, parentMenu, _menuItems);
    }
}

