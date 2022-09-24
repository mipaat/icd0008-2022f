namespace MenuSystem;

public class MenuItem
{
    public readonly string Text;
    private Func<Menu?, EMenuFunction> _action;
    public EMenuFunction Type;

    public MenuItem(string text, Func<Menu?, EMenuFunction> action, EMenuFunction type = EMenuFunction.Continue)
    {
        Text = text;
        _action = action;
        Type = type;
    }

    public MenuItem(string text, Func<Menu?, Menu> menuCreator)
    {
        Text = text;
        Type = EMenuFunction.Continue;
        _action = parentMenu =>
        {
            var newMenu = menuCreator(parentMenu);
            return newMenu.Run();
        };
    }

    public EMenuFunction Run(Menu? menu = null)
    {
        return Type is EMenuFunction.Back or EMenuFunction.Exit or EMenuFunction.Main ? Type : _action(menu);
    }

    public override string ToString()
    {
        return Text;
    }
}