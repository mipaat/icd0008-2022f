namespace Domain;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class ExpectedNotNull : Attribute
{
}