using ConsoleMenuSystem;

namespace ConsoleClient;

public static class MenuParameter
{
    public static bool ParameterEquals(Menu menu, string menuId, Enum expectedParameterValue)
    {
        var value = menu.GetSelectedItemId(menuId);
        return value is not null && value == expectedParameterValue.ToString();
    }
}

public enum GameType
{
    Local,
    Online
}

public enum OpponentType
{
    Ai,
    Player
}