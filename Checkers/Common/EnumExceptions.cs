namespace Common;

public class EnumFailedConvertException : Exception
{
    public EnumFailedConvertException(int? value, Type enumType) : base($"Failed to convert {value?.ToString() ?? "null"} to {enumType}!")
    {
    }
}

public class EnumEmptyException : Exception
{
    public EnumEmptyException(Type enumType) : base($"Enum type {enumType} has 0 members!")
    {
    }
}