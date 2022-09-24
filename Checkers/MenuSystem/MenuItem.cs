namespace MenuSystem;

public class MenuItem
{
    public readonly string Text;
    private readonly Func<Menu?, EMenuFunction> _action;

    public MenuItem(string text, EMenuFunction type) : this(text, _ => type)
    {
    }

    public MenuItem(string text, Action action) : this(text, _ =>
    {
        action();
        return EMenuFunction.Continue;
    })
    {
    }

    public MenuItem(string text, Func<Menu?, EMenuFunction> action)
    {
        Text = text;
        _action = action;
    }

    public MenuItem(string text, Func<Menu?, Menu> menuCreator)
    {
        Text = text;
        _action = parentMenu =>
        {
            var newMenu = menuCreator(parentMenu);
            return newMenu.Run();
        };
    }

    public EMenuFunction Run(Menu? menu = null)
    {
        return _action(menu);
    }

    public override string ToString()
    {
        return Text;
    }
}