using DAL.Filters;

namespace WebApp.MyLibraries.ModelBinders;

public class CompletionFilterModelBinder : CustomModelBinder<CompletionFilter>
{
    protected override CompletionFilter DefaultValue => CompletionFilter.Default;

    protected override Func<string?, CompletionFilter?> Construct =>
        input => CompletionFilter.Construct(input, CompletionFilter.Values);
}