namespace Common;

public static class EnumUtils
{
    public static int? GetValueNullable<T>(T? @enum) where T : struct, Enum
    {
        if (@enum == null) return null;
        return GetValue(@enum.Value);
    }
    
    public static int GetValue<T>(T @enum) where T : struct, Enum
    {
        var intValue = Convert.ToInt32(@enum);
        if (IsExtension<T>()) intValue = -intValue - 1;
        return intValue;
    }
    
    private static List<int> GetValues<T>() where T : struct, Enum
    {
        var result = new List<int>();
        foreach (var @enum in Enum.GetValues<T>())
        {
            result.Add(GetValue(@enum));
        }

        return result;
    }

    public static IEnumerable<int> GetValues<T, TE>() where T : struct, Enum where TE : struct, Enum
    {
        var typesAreSame = typeof(T) == typeof(TE);
        if (!(IsExtension<T>() ^ IsExtension<TE>()) && !typesAreSame)
            throw new ArgumentException(
                $"Exactly 1 of {typeof(T)} and {typeof(TE)} should have the {typeof(EnumIsExtensionAttribute)} attribute!");
        var result = GetValues<T>();
        if (!typesAreSame) result.AddRange(GetValues<TE>());
        return result;
    }

    private static bool IsExtension<T>() where T : struct, Enum
    {
        var enumType = typeof(T);
        var attributes = enumType.GetCustomAttributes(typeof(EnumIsExtensionAttribute), false);
        return attributes.Length > 0;
    }

    public static T? GetEnum<T>(int? value) where T : struct, Enum
    {
        if (TryGetEnum(value, out T? result))
        {
            return result;
        }

        throw new EnumFailedConvertException(value, typeof(T));
    }
    
    public static bool TryGetEnum<T>(int? value, out T? @enum) where T : struct, Enum
    {
        if (value == null)
        {
            @enum = null;
            return true;
        }

        if (IsExtension<T>()) value = -(value + 1);

        if (Enum.IsDefined(typeof(T), value))
        {
            @enum = (T)(object)value;
            return true;
        }

        @enum = null;
        return false;
    }

    public static string GetDisplayString<T, TE>(int value) where T : struct, Enum where TE : struct, Enum
    {
        try
        {
            return GetDisplayString<T>(value);
        }
        catch (EnumFailedConvertException)
        {
            return GetDisplayString<TE>(value);
        }
    }
    
    public static string GetDisplayString<T>(int value) where T : struct, Enum
    {
        var @enum = GetEnum<T>(value);
        if (@enum == null) throw new EnumFailedConvertException(value, typeof(T));
        return GetDisplayString(@enum.Value);
    }
    
    public static string GetDisplayString<T>(T @enum) where T : struct, Enum
    {
        var result = @enum.ToString();
        var enumType = typeof(T);
        var memberInfos = enumType.GetMember(result);
        if (memberInfos.Length > 0)
        {
            var attributes = memberInfos[0].GetCustomAttributes(typeof(EnumDisplayNameAttribute), false);
            if (attributes.Length > 0)
            {
                return ((EnumDisplayNameAttribute)attributes[0]).Text;
            }
        }

        return result;
    }

    public static bool TryGetDefaultValue<T>(out T? @enum) where T : struct, Enum
    {
        var enumType = typeof(T);
        var names = Enum.GetNames<T>();
        T? result = null;
        foreach (var name in names)
        {
            var memberInfos = enumType.GetMember(name);
            foreach (var memberInfo in memberInfos)
            {
                var attributes = memberInfo.GetCustomAttributes(typeof(EnumDefaultValueAttribute), false);
                foreach (var _ in attributes)
                {
                    result = Enum.Parse<T>(name);
                }
            }
        }

        if (result == null)
        {
            @enum = null;
            return false;
        }

        @enum = result;
        return true;
    }

    public static T GetDefaultValue<T>() where T : struct, Enum
    {
        if (TryGetDefaultValue<T>(out var result)) return result!.Value;
        var names = Enum.GetNames<T>();
        if (names.Length == 0) throw new EnumEmptyException(typeof(T));
        return Enum.Parse<T>(names[0]);
    }
}