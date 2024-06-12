using HabitTracker.Authentication;
using HabitTracker.Models;
using HabitTracker.Services;
using HabitTracker.Auth;

namespace HabitTracker.Helpers;

public static class StartupExtensionMethods
{
    public static void AddControllersAndBinders(this IServiceCollection services)
    {
         services.AddControllers(options =>
     {
         // add user binder
         options.ModelBinderProviders.Insert(0, new UserModelBinderProvider());
    // necessery to make it work when called from other (test) assembly
     }).AddApplicationPart(typeof(HabitTrackerContext).Assembly);;
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
