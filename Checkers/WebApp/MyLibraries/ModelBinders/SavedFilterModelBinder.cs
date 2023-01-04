using DAL.Filters;

namespace WebApp.MyLibraries.ModelBinders;

public class SavedFilterModelBinder : CustomModelBinder<SavedFilter>
{
    protected override SavedFilter DefaultValue => SavedFilter.Default;

    protected override Func<string?, SavedFilter?> Construct =>
        input => SavedFilter.Construct(input, SavedFilter.Values);
}