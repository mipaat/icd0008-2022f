namespace Common;

[AttributeUsage(AttributeTargets.Enum)]
public abstract class EnumAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Field)]
public abstract class EnumFieldAttribute : Attribute
{
}

public class EnumIsExtensionAttribute : EnumAttribute
{
}

public class EnumDefaultValueAttribute : EnumFieldAttribute
{
}

public class EnumDisplayNameAttribute : EnumFieldAttribute
{
    public string Text { get; }

    public EnumDisplayNameAttribute(string displayName)
    {
        Text = displayName;
    }
}