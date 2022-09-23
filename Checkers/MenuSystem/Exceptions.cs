namespace MenuSystem;

public class ExitMenuException : Exception {}

public class BackMenuException : Exception {}

public class MainMenuException : Exception {}

public class SelectMenuException : Exception
{
    private Action<Menu> _action;
    
    public SelectMenuException(Action<Menu> action)
    {
        _action = action;
    }

    public void Run(Menu menu)
    {
        _action(menu);
    }
}
