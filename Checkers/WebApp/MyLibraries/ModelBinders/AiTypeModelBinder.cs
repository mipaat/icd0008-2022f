using DAL.Filters;

namespace WebApp.MyLibraries.ModelBinders;

public class AiTypeModelBinder : CustomModelBinder<AiTypeFilter>
{
    protected override AiTypeFilter DefaultValue => AiTypeFilter.Default;

    protected override Func<string?, AiTypeFilter?> Construct =>
        input => AiTypeFilter.Construct(input, AiTypeFilter.Values);
}