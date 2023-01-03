using System.Reflection;

namespace Domain;

public abstract class AbstractDatabaseEntity : IDatabaseEntity
{
    public int Id { get; set; }

    public void Refresh(IDatabaseEntity other, bool partial = false)
    {
        if (other.GetType() != GetType()) return;
        const BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        var fields = other.GetType()
            .GetProperties(bindingAttr);
        foreach (var field in fields)
        {
            if (field.GetSetMethod() == null) continue;
            var overrideNull = (partial && Attribute.IsDefined(field, typeof(ExpectedNotNull))) ||
                               Attribute.IsDefined(field, typeof(PreferNotNull));
            var thisField = GetType().GetProperty(field.Name, bindingAttr);
            if (thisField == null) continue;
            var otherValue = field.GetValue(other);
            if (overrideNull && otherValue == null) continue;
            thisField.SetValue(this, otherValue);
        }
    }

    public override string ToString()
    {
        var propertyStrings = new List<string>();

        foreach (var propertyInfo in GetType().GetProperties())
            if (propertyInfo.CanRead)
                propertyStrings.Add(propertyInfo.Name + ": " + (propertyInfo.GetValue(this) ?? "null"));

        return GetType().Name + "(" + string.Join(", ", propertyStrings) + ")";
    }
}