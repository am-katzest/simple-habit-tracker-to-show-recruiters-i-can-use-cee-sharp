using HabitTracker.Helpers;
using HabitTracker.Services;
using Microsoft.AspNetCore.TestHost;

namespace HabitTracker.Tests;

public class HostFixture : CreatedDatabaseFixture
{
    // doing all the setup takes ~10ms
    public IHost makeHost()
    {
        return new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .ConfigureServices(services =>
                    {

                        services.AddControllers().AddApplicationPart(typeof(HabitTracker.Models.HabitTrackerContext).Assembly);
                        services.RegisterServices(MakeContext, new RealClock());
                    }).Configure(x =>
                    {
                        x.UseAuthentication();
                        x.UseRouting();
                        x.UseEndpoints(e => e.MapControllers());
                    });
            })
            .Start();

    }
}
