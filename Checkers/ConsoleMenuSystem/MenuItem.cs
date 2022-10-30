namespace ConsoleMenuSystem;

public class MenuItem
{
    public string Text;
    public readonly string? Id;
    private readonly Func<Menu, EMenuFunction> _action;

    public MenuItem(string text, Func<Menu, EMenuFunction> action, string? id = null)
    {
        Text = text;
        _action = action;
        Id = id;
    }

    public MenuItem(string text, EMenuFunction type, string? id = null) : this(text, _ => type, id)
    {
    }

    public MenuItem(string text, Action action, string? id = null) : this(text, _ =>
    {
        action();
        return EMenuFunction.Continue;
    }, id)
    {
    }

    public MenuItem(string text, MenuFactory menuFactory, string? id = null) : this(text, parentMenu =>
    {
        var newMenu = menuFactory.Create(parentMenu.ConsoleWindow, parentMenu);
        return newMenu.Run();
    }, id)
    {
    }

    public EMenuFunction Run(Menu menu)
    {
        return _action(menu);
    }

    public override string ToString()
    {
        return $"MenuItem('{Text}')";
    }
}