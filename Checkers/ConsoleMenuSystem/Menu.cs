﻿using ConsoleUI;

namespace ConsoleMenuSystem;

public class Menu
{
    public static readonly MenuItem Exit = new("Exit", EMenuFunction.Exit);
    public static readonly MenuItem Back = new("Back", EMenuFunction.Back);
    public static readonly MenuItem MainMenu = new("Main Menu", EMenuFunction.MainMenu);
    private const int Height = 12;

    public readonly ConsoleWindow ConsoleWindow;
    public readonly Menu? ParentMenu;

    private readonly Func<Menu, string> _titleFunc;
    public string Title => _titleFunc(this);

    public bool AppendDefaultMenuItems = true;

    private readonly Func<Menu, List<MenuItem>> _menuItemsFunc;
    private List<MenuItem>? _menuItemsCache;

    public bool IsExitable = true;

    public string? CustomHeader = null;
    public CustomMenuItemsCacheModifier? CustomBehaviour;

    public List<MenuItem> MenuItems
    {
        get
        {
            if (_menuItemsCache == null)
            {
                _menuItemsCache = _menuItemsFunc(this);
            }

            var result = _menuItemsCache.ToList();

            if (AppendDefaultMenuItems) result.AddRange(BuiltInMenuItems);

            if (result.Count == 0) throw new Exception("Menu must contain at least one item!");

            return result;
        }
    }

    public MenuItem SelectedMenuItem => MenuItems[CursorPosition];

    public List<MenuItem> BuiltInMenuItems
    {
        get
        {
            var result = new List<MenuItem>();
            if (ParentMenu is not null) result.Add(Back);
            if (Hierarchy.Count > 2) result.Add(MainMenu);
            result.Add(Exit);
            return result;
        }
    }

    public List<Menu> Hierarchy
    {
        get
        {
            var result = new List<Menu>();
            if (ParentMenu is not null)
            {
                result.AddRange(ParentMenu.Hierarchy);
            }

            result.Add(this);
            return result;
        }
    }

    private int _cursorPosition;

    public int CursorPosition
    {
        get
        {
            _cursorPosition = Math.Max(Math.Min(_cursorPosition, MenuItems.Count - 1), 0);
            return _cursorPosition;
        }
        set
        {
            var menuItems = MenuItems;
            if (value < 0 || value >= menuItems.Count)
                throw new ArgumentOutOfRangeException(
                    $"Can't move cursor from {_cursorPosition} to {value} - out of Menu bounds ({0} - {menuItems.Count - 1})!");
            _cursorPosition = value;
        }
    }

    public string MenuPath => (ParentMenu?.MenuPath ?? "") + Title + "/";

    public Menu(ConsoleWindow consoleWindow,
        Func<Menu, string> titleFunc,
        Func<Menu, List<MenuItem>> menuItemsFunc,
        Menu? parentMenu = null)
    {
        ConsoleWindow = consoleWindow;
        _titleFunc = titleFunc;
        _menuItemsFunc = menuItemsFunc;
        ParentMenu = parentMenu;
    }

    public int IncrementCursorPosition(int amount = 1)
    {
        var menuItems = MenuItems;
        amount %= menuItems.Count;
        if (amount < 0) amount = menuItems.Count + amount;
        CursorPosition = (CursorPosition + amount) % MenuItems.Count;
        return CursorPosition;
    }

    public int DecrementCursorPosition(int amount = 1)
    {
        return IncrementCursorPosition(-amount);
    }

    private void WriteMenuItems(int start, int end)
    {
        var menuItems = MenuItems;
        for (var i = start; i < end; i++)
        {
            if (i < menuItems.Count)
            {
                var menuItem = menuItems[i];
                ConsoleWindow.AddLine(menuItem.Text, backgroundColor: i == CursorPosition ? ConsoleColor.White : null);
            }
            else
            {
                ConsoleWindow.AddLine();
            }
        }
    }

    public void ClearMenuItemsCache()
    {
        _menuItemsCache = null;
    }

    private int LongestLine()
    {
        return Math.Max(MenuItems.Aggregate(0, (i, item) => Math.Max(i, item.Text.Length)), Title.Length);
    }

    private int MenuItemsHeight =>
        Height - (2 + (CustomHeader != null ? 1 : 0)); // -2 for the surrounding separator lines

    private int Page
    {
        get => CursorPosition / MenuItemsHeight;
        set
        {
            var normalizedValue = value;
            normalizedValue %= MaxPage + 1;
            if (normalizedValue < 0) normalizedValue = MaxPage + normalizedValue + 1;
            var relativePositionOnPage = CursorPosition - MenuItemsStart;
            CursorPosition = normalizedValue * MenuItemsHeight;
            CursorPosition = Math.Min(CursorPosition + relativePositionOnPage, MenuItems.Count - 1);
        }
    }
    private int MenuItemsStart => Page * MenuItemsHeight;
    private int MaxPage => (MenuItems.Count - 1) / MenuItemsHeight;

    private EMenuFunction MenuLoop()
    {
        do
        {
            ConsoleWindow.AddLine(Title);

            if (CustomHeader != null)
            {
                ConsoleWindow.AddLine(CustomHeader.ReplaceLineEndings("").Replace('\r', '<'));
            }

            ConsoleWindow.AddLinePattern(Page == 0 ? "_" : "▲", Math.Max(LongestLine(), 20));
            WriteMenuItems(MenuItemsStart, MenuItemsStart + MenuItemsHeight);

            ConsoleWindow.AddLinePattern(Page < MaxPage ? "▼" : "_", Math.Max(LongestLine(), 20));

            ConsoleWindow.Render();

            var input = ConsoleWindow.AwaitKeyInput();
            switch (input.KeyInfo?.Key)
            {
                case ConsoleKey.DownArrow:
                    IncrementCursorPosition();
                    break;
                case ConsoleKey.UpArrow:
                    DecrementCursorPosition();
                    break;
                case ConsoleKey.Escape:
                case ConsoleKey.Backspace:
                    if (IsExitable)
                    {
                        ClearMenuItemsCache();
                        return EMenuFunction.Continue;
                    }
                    break;
                case ConsoleKey.RightArrow:
                    Page++;
                    break;
                case ConsoleKey.LeftArrow:
                    Page--;
                    break;
                case ConsoleKey.Enter:
                    var menuFunction = SelectedMenuItem!.Run(this);
                    ClearMenuItemsCache();
                    switch (menuFunction)
                    {
                        case EMenuFunction.Back:
                            return EMenuFunction.Continue;
                        case EMenuFunction.Exit:
                            return menuFunction;
                        case EMenuFunction.MainMenu:
                            if (ParentMenu is not null) return menuFunction;
                            break;
                        case EMenuFunction.Refresh:
                            return menuFunction;
                    }

                    break;
                default:
                    SelectedMenuItem?.InvokeCustomCallback(input, this);
                    CustomBehaviour?.Invoke(input, ref _menuItemsCache);
                    break;
            }
        } while (true);
    }

    public EMenuFunction Run()
    {
        var result = MenuLoop();
        ClearMenuItemsCache();
        return result;
    }

    public override string ToString()
    {
        return $"Menu({Title}, MenuItems: {MenuItems.Count})";
    }
}