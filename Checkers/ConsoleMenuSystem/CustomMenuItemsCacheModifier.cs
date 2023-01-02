using ConsoleUI;

namespace ConsoleMenuSystem;

public delegate void CustomMenuItemsCacheModifier(
    ConsoleInput consoleInput,
    Menu menu,
    ref List<MenuItem>? menuItemsCache
);