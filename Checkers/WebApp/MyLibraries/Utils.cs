using Domain;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.MyLibraries;

public static class Utils
{
    private static IEnumerable<SelectListItem> EnumOptions(Type enumType, SelectListItem? emptyOption = null)
    {
        if (!enumType.IsEnum)
            throw new ArgumentException($"Argument {nameof(enumType)} must be an Enum, but was {enumType}");
        var result = new List<SelectListItem>();
        var enumValues = Enum.GetValues(enumType);
        if (emptyOption != null) result.Add(emptyOption);
        foreach (var value in enumValues)
        {
            result.Add(new SelectListItem(value.ToString(), ((int)value).ToString()));
        }

        return result;
    }

    public static IEnumerable<SelectListItem> AiTypeOptions =>
        EnumOptions(typeof(EAiType), new SelectListItem("No AI", ""));

    public static IEnumerable<SelectListItem> PlayerColorOptions =>
        EnumOptions(typeof(EPlayerColor), new SelectListItem("", ""));

    public static string ShortenSerializedGamePieces(CheckersState checkersState)
    {
        return checkersState.SerializedGamePieces.Length > 50
            ? checkersState.SerializedGamePieces[..47] + "..."
            : checkersState.SerializedGamePieces;
    }
}