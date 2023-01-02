using Common;
using Domain;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.MyLibraries;

public static class Utils
{
    public static IEnumerable<SelectListItem> EnumOptions<T, TE>(int? selected)
        where T : struct, Enum where TE : struct, Enum
    {
        var result = new List<SelectListItem>();
        var enumValues = EnumUtils.GetValues<T, TE>();
        foreach (var value in enumValues)
        {
            result.Add(new SelectListItem(EnumUtils.GetDisplayString<T, TE>(value), value.ToString(), value == selected));
        }

        return result;
    }

    public static IEnumerable<SelectListItem> EnumOptions<T>(T? selected) where T : struct, Enum
    {
        return EnumOptions(null, selected);
    }

    public static IEnumerable<SelectListItem> EnumOptions<T>(SelectListItem? emptyOption = null, T? selected = null)
        where T : struct, Enum
    {
        var result = new List<SelectListItem>();
        var enumValues = Enum.GetValues<T>();
        if (emptyOption != null) result.Add(emptyOption);
        foreach (var value in enumValues)
        {
            result.Add(new SelectListItem(EnumUtils.GetDisplayString(value), EnumUtils.GetValue(value).ToString(),
                !(emptyOption?.Selected ?? false) && value.Equals(selected)));
        }

        return result;
    }

    public static IEnumerable<SelectListItem> AiTypeOptionsSelected(EAiType? selected = null)
    {
        return EnumOptionsOtherEmpty(EAiTypeNullable.None, selected);
    }

    public static IEnumerable<SelectListItem> EnumOptionsOtherEmpty<T, TEmpty>(TEmpty empty, T? selected = null) where T : struct, Enum where TEmpty : struct, Enum
    {
        var emptyText = EnumUtils.GetDisplayString(empty);
        return EnumOptions(new SelectListItem(emptyText, ""), selected);
    }

    public static IEnumerable<SelectListItem> PlayerColorOptions(EPlayerColor? selected) =>
        EnumOptionsOtherEmpty(EPlayerColorNullable.None, selected);

    public static string ShortenSerializedGamePieces(CheckersState checkersState)
    {
        return checkersState.SerializedGamePieces.Length > 50
            ? checkersState.SerializedGamePieces[..47] + "..."
            : checkersState.SerializedGamePieces;
    }
}