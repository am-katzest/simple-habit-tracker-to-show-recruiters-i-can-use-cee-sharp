//authentication puts user into context.Items
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace HabitTracker.Auth;

public class UserModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        bindingContext.Result = bindingContext.HttpContext.Items["user"] switch
        {
            DTOs.UserId user_id => ModelBindingResult.Success(user_id),
            _ => ModelBindingResult.Failed(),
        };
        // bindingContext.Result = ModelBindingResult.Success(null!);
        return Task.CompletedTask;
    }
}

public class UserModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context) =>
        context.Metadata.ModelType == typeof(DTOs.UserId) ? new UserModelBinder() : null;
}
