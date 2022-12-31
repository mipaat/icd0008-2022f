namespace ConsoleUI;

public record ConsoleKeyInfoBasic(ConsoleKey Key)
{
    private ConsoleModifiers Mods { get; } = 0;

    public ConsoleKeyInfoBasic(ConsoleKey key, bool shift, bool alt, bool control) : this(key)
    {
        if (shift)
            Mods |= ConsoleModifiers.Shift;
        if (alt)
            Mods |= ConsoleModifiers.Alt;
        if (control)
            Mods |= ConsoleModifiers.Control;
    }

    public bool Equals(ConsoleKeyInfo? consoleKeyInfo)
    {
        if (consoleKeyInfo == null) return false;
        return consoleKeyInfo.Value.Modifiers == Mods && consoleKeyInfo.Value.Key == Key;
    }
}