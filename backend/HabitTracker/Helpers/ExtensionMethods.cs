using HabitTracker.Authentication;
using HabitTracker.Models;
using HabitTracker.Services;

namespace HabitTracker.Helpers;

public static class SetupExtensionMethods
{
    public static void AddControllersAndBinders(this IServiceCollection services)
    {
        services.AddControllers(options =>
     {
         // add user binder
         options.ModelBinderProviders.Insert(0, new Controllers.UserModelBinderProvider());
     });
    }
    public static void RegisterServices(this IServiceCollection services, Func<HabitTrackerContext> db, IClock clock)
    {

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<HabitTrackerContext>((_) => db());
        services.AddSingleton(clock);
    }
    public static void RegisterServicesProduction(this IServiceCollection services)
    {
        var str = ConnectionDetails.FromEnvironment().Format();
        services.RegisterServices(() => new(str), new RealClock());
    }
    public static void AddAuth(this IServiceCollection services)
    {
        services.AddAuthentication("Local").AddScheme<LocalAuthenticationOptions, LocalAuthenticationHandler>("Local", null);
        services.AddAuthorization();
    }
}
