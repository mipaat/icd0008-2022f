using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WebApp.MyLibraries.ModelBinders;

public interface ICustomModelBinder : IModelBinder
{
    public Type Type { get; }
}

public abstract class CustomModelBinder<T> : ICustomModelBinder
{
    protected abstract T DefaultValue { get; }
    protected abstract Func<string?, T?> Construct { get; }
    public Type Type => typeof(T);

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null) throw new ArgumentNullException(nameof(bindingContext));

        var modelName = bindingContext.ModelName;
        var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

        var model = DefaultValue;

        if (valueProviderResult != ValueProviderResult.None)
        {
            var modelState = bindingContext.ModelState;
            modelState.SetModelValue(modelName, valueProviderResult);

            var value = valueProviderResult.FirstValue;

            model = ConstructOrDefault(value)!;
        }

        bindingContext.Result = ModelBindingResult.Success(model);

        return Task.CompletedTask;
    }

    private T ConstructOrDefault(string? value)
    {
        return Construct(value) ?? DefaultValue;
    }
}