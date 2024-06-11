//authentication puts user into context.Items
using HabitTracker.DTOs.User;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace HabitTracker.Auth;

public class UserModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        bindingContext.Result = bindingContext.HttpContext.Items["user"] switch
        {
            IdOnly user_id => ModelBindingResult.Success(user_id),
            _ => ModelBindingResult.Failed(),
        };
        // bindingContext.Result = ModelBindingResult.Success(null!);
        return Task.CompletedTask;
    }
}

public class UserModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context) =>
        context.Metadata.ModelType == typeof(IdOnly) ? new UserModelBinder() : null;
}
