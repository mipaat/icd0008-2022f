using ConsoleUI;

namespace ConsoleMenuSystem;

public delegate void CustomMenuItemsCacheModifier(
    ConsoleInput consoleInput,
    ref List<MenuItem>? menuItemsCache
);