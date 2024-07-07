using HabitTracker.Helpers;
using HabitTracker.Services;
using Microsoft.AspNetCore.TestHost;

namespace HabitTracker.Tests;

public class HostFixture : CreatedDatabaseFixture
{
    private IHost host;

    public HostFixture()
    {
        host = makeHost();
    }

    // doing all the setup takes ~10ms
    private IHost makeHost()
    {
        return new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .ConfigureServices(services =>
                    {

                        services.AddControllersAndBinders();
                        services.RegisterServices(MakeContext, new RealClock());
                        services.AddAuth();
                    }).Configure(x =>
                    {
                        x.UseAuthentication();
                        x.UseRouting();
                        x.UseAuthorization();
                        x.AddMiddleware();
                        x.UseEndpoints(e => e.MapControllers());
                    });
            })
            .Start();
    }
    public HttpClient Client { get => host.GetTestClient(); }
}
