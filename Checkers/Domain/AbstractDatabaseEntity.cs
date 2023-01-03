namespace Domain;

public abstract class AbstractDatabaseEntity : IDatabaseEntity
{
    public int Id { get; set; }

    public override string ToString()
    {
        var propertyStrings = new List<string>();

        foreach (var propertyInfo in GetType().GetProperties())
            if (propertyInfo.CanRead)
                propertyStrings.Add(propertyInfo.Name + ": " + (propertyInfo.GetValue(this) ?? "null"));

        return GetType().Name + "(" + string.Join(", ", propertyStrings) + ")";
    }
}