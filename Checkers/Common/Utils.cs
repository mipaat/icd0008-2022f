namespace Common;

public class Utils
{
    private static bool StringContains(string? query, bool all, params string?[] comparisonStrings)
    {
        var normalizedQuery = query?.ToLower().Trim() ?? "";
        if (normalizedQuery.Length == 0) return true;
        var result = true;
        foreach (var compare in comparisonStrings)
        {
            var normalizedCompare = compare?.ToLower().Trim() ?? "";
            var comparisonResult = normalizedCompare.Contains(normalizedQuery);
            result = result && comparisonResult;
            if (all && !comparisonResult) return false;
            if (!all && comparisonResult) return true;
        }

        return result;
    }
    
    public static bool StringAnyContain(string? query, params string?[] comparisonStrings)
    {
        return StringContains(query, false, comparisonStrings);
    }

    public static bool StringAllContain(string? query, params string?[] comparisonStrings)
    {
        return StringContains(query, true, comparisonStrings);
    }
}