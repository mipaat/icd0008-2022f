namespace Common;

public enum EAiType
{
    Random,
    Simple,
    SimpleMinMax
}

[EnumIsExtension]
public enum EAiTypeExtended
{
    [EnumDefaultValue]
    All,
    [EnumDisplayName("No AI")]
    NoAi
}

[EnumIsExtension]
public enum EAiTypeNullable
{
    [EnumDisplayName("")]
    None
}