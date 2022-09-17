namespace MenuSystem;

public class MenuItem
{
    public readonly string Text;
    public string Shortcut { get; set; }
    public Action<string> Action;

    public MenuItem(string text, Action<string> action, string? preferredShortcut = null)
    {
        Text = text;
        preferredShortcut ??= GeneratePreferredShortcut(text);
        Shortcut = preferredShortcut.ToLower();
        Action = action;
    }

    private static string GeneratePreferredShortcut(string text)
    {
        return text.ElementAt(0).ToString();
    }

    public void Run(string input)
    {
        Action(input);
    }

    public override string ToString()
    {
        return Shortcut + ") " + Text;
    }

    public static Func<MenuItem> MenuItemCreator(string text, Func<Menu?, Menu> menuCreator, string? preferredShortcut = null)
    {
        void CreateAndRunMenu(string s)
        {
            throw new SelectMenuException((parentMenu =>
            {
                var menu = menuCreator(parentMenu);
                menu.RunMenu();
            }));
        }

        return MenuItemCreator(text, CreateAndRunMenu, preferredShortcut);
    }

    public static Func<MenuItem> MenuItemCreator(string text, Action<string> action, string? preferredShortcut = null)
    {
        return () => new MenuItem(text, action, preferredShortcut);
    }
}