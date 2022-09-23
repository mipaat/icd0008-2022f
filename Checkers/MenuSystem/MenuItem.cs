namespace MenuSystem;

public class MenuItem
{
    public readonly string Text;
    public Action Action;

    public MenuItem(string text, Action action)
    {
        Text = text;
        Action = action;
    }

    public void Run()
    {
        Action();
    }

    public override string ToString()
    {
        return Text;
    }

    public static Func<MenuItem> MenuItemCreator(string text, Func<Menu?, Menu> menuCreator)
    {
        void CreateAndRunMenu()
        {
            throw new SelectMenuException((parentMenu =>
            {
                var menu = menuCreator(parentMenu);
                menu.RunMenu();
            }));
        }

        return MenuItemCreator(text, CreateAndRunMenu);
    }

    public static Func<MenuItem> MenuItemCreator(string text, Action action)
    {
        return () => new MenuItem(text, action);
    }
}