
using System.Net.Http.Headers;
using HabitTracker.Authentication;
using HabitTracker.Services;
using HabitTracker.Tests;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class AuthenticationTest(CreatedDatabaseFixture Fixture) : IClassFixture<CreatedDatabaseFixture>
{

    private IUserService MakeService() => MakeService(new RealClock());
    private IUserService MakeService(IClock clock) => new UserService(Fixture.MakeContext(), clock);

    private IHost makeHost()
    {
        return new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .ConfigureServices(services =>
                    {
                        services.AddRouting();
                        services.AddScoped<IUserService>(st => MakeService());
                        services.AddAuthentication("Local").AddScheme<LocalAuthenticationOptions, LocalAuthenticationHandler>("Local", null);
                        services.AddAuthorization();
                    })
                    .Configure(app =>
                    {
                        app.UseAuthentication();
                        app.UseRouting();
                        app.UseAuthorization();

                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapGet("/auth", async c =>
                            {
                                var uname = c.Items["user"] switch
                                {
                                    User u => u.DisplayName,
                                    _ => "guh"
                                };
                                await c.Response.WriteAsync(uname);
                            }).RequireAuthorization();
                            endpoints.MapGet("/noauth", () =>
                                TypedResults.Text("test"));
                        });
                    });
            })
            .Start();
    }

    [Fact]
    public async void Negative()
    {
        MakeService().createPasswordUser("a", "b");
        var t = MakeService().createToken("a", "b");
        Assert.NotNull(MakeService().validateToken(t));
        using var h = makeHost();
        var c = h.GetTestClient();
        Assert.Equal(System.Net.HttpStatusCode.OK, (await c.GetAsync("/noauth")).StatusCode);
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, (await c.GetAsync("/auth")).StatusCode);
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("meow!");
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, (await c.GetAsync("/auth")).StatusCode);
    }

    [Fact]
    public async void Positive()
    {
        MakeService().createPasswordUser("meow", "b");
        var t = MakeService().createToken("meow", "b");
        Assert.NotNull(MakeService().validateToken(t));
        using var h = makeHost();
        var c = h.GetTestClient();
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(t);
        var correct = await c.GetAsync("/auth");
        Assert.Equal(System.Net.HttpStatusCode.OK, correct.StatusCode);
        Assert.Equal("meow", await correct.Content.ReadAsStringAsync());
    }
}
