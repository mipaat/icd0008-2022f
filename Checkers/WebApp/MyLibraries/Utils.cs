using DAL.Filters;
using Domain.Model;
using GameBrain;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.MyLibraries;

public static class Utils
{
    public static IEnumerable<SelectListItem> GetSelectOptions<T>(IEnumerable<T> items, T selectedItem)
        where T : IFilter
    {
        return GetSelectOptions(items, selectedItem, filter => filter.Identifier,
            filter => filter.DisplayString ?? filter.Identifier);
    }

    public static IEnumerable<SelectListItem> GetSelectOptions<T>(IEnumerable<T> items, T selectedItem,
        Func<T, string> identifierFunc, Func<T, string>? customToString = null)
    {
        var result = new List<SelectListItem>();
        foreach (var item in items)
            result.Add(new SelectListItem(customToString?.Invoke(item) ?? item?.ToString() ?? "",
                identifierFunc.Invoke(item), item?.Equals(selectedItem) ?? false));

        return result;
    }

    public static IEnumerable<SelectListItem> GetSelectOptionsNullable<T>(T? selected) where T : struct, Enum
    {
        return GetSelectOptions(selected, "");
    }

    public static IEnumerable<SelectListItem> GetSelectOptions<T>(T? selected, string nullOptionText)
        where T : struct, Enum
    {
        return GetSelectOptions(selected, new SelectListItem(nullOptionText, "", selected == null));
    }

    public static IEnumerable<SelectListItem> GetSelectOptions<T>(T? selected, SelectListItem? nullOption = null)
        where T : struct, Enum
    {
        var result = new List<SelectListItem>();
        if (nullOption != null) result.Add(nullOption);
        foreach (var value in Enum.GetValues<T>())
            result.Add(new SelectListItem(value.ToString(), ((int)(object)value).ToString(), value.Equals(selected)));

        return result;
    }

    public static string ShortenSerializedGamePieces(CheckersState checkersState)
    {
        return checkersState.SerializedGamePieces.Length > 50
            ? checkersState.SerializedGamePieces[..47] + "..."
            : checkersState.SerializedGamePieces;
    }
    
    public static string GetBoardCellBackgroundClass(int x, int y)
    {
        return CheckersBrain.IsSquareBlack(x, y) ? "checkers-cell-black" : "checkers-cell-white";
    }
}