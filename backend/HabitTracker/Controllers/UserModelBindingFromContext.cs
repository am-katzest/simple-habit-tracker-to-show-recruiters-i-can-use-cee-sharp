//authentication puts user into context.Items
using HabitTracker.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace HabitTracker.Controllers;

public class UserModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        bindingContext.Result = bindingContext.HttpContext.Items["user"] switch
        {
            User user => ModelBindingResult.Success(user),
            _ => ModelBindingResult.Failed(),
        };
        return Task.CompletedTask;
    }
}

public class UserModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context) =>
        context.Metadata.ModelType == typeof(User) ? new UserModelBinder() : null;
}
