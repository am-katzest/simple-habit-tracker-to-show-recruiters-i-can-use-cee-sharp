using HabitTracker.Helpers;
using HabitTracker.Models;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersAndBinders();
builder.Services.RegisterServicesProduction();
builder.Services.AddAuth();

var app = builder.Build();
{
    var ctx = app.Services.GetRequiredService<HabitTrackerContext>();
    ctx.Database.EnsureCreated(); // temporary
}
app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();
app.UseHttpsRedirection();
app.AddMiddleware();
app.UseEndpoints(e => e.MapControllers());
app.Run();
