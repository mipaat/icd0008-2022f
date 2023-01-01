using ConsoleUI;

namespace ConsoleMenuSystem;

public class MenuItem
{
    public string Text;
    private readonly Func<Menu, MenuItem, EMenuFunction> _action;
    public CustomMenuItemCallback? CustomCallBack = null;

    public MenuItem(string text, Func<Menu, MenuItem, EMenuFunction> action)
    {
        Text = text;
        _action = action;
    }

    public MenuItem(string text, Func<Menu, EMenuFunction> action)
        : this(text, (m, _) => action(m))
    {
    }

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

    public MenuItem(string text, Func<EMenuFunction> action) : this(text, _ => action())
    {
    }

    public MenuItem(string text, MenuFactory menuFactory) : this(text, parentMenu =>
    {
        var newMenu = menuFactory.Create(parentMenu.ConsoleWindow, parentMenu);
        return newMenu.Run();
    })
    {
    }

    public EMenuFunction Run(Menu menu)
    {
        return _action(menu, this);
    }

    public void InvokeCustomCallback(ConsoleInput input, Menu menu)
    {
        CustomCallBack?.Invoke(input, menu, this);
    }

    public override string ToString()
    {
        return $"MenuItem('{Text}')";
    }
}