namespace Common;

public enum EPlayerColor
{
    White,
    Black
}

[EnumIsExtension]
public enum EPlayerColorNullable
{
    [EnumDisplayName("")]
    [EnumDefaultValue]
    None
}