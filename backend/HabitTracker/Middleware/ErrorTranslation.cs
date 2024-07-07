using HabitTracker.Exceptions;
namespace HabitTracker.Middleware;
using Microsoft.AspNetCore.Http;

public class ErrorTranslation(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (UserVisibleException e)
        {
            var r = context.Response;
            if (r.HasStarted)
            {
                throw new Exception("Both UserVisibleException has been thrown and response written");
            }
            r.StatusCode = (int)e.Code;
            await r.WriteAsync(e.ErrorMessage);
        }
    }
}
