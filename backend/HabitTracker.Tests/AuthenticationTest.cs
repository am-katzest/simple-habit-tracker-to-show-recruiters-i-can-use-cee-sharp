
using System.Net.Http.Headers;
using HabitTracker.Authentication;
using HabitTracker.DTOs;
using HabitTracker.Services;
using Microsoft.AspNetCore.TestHost;

namespace HabitTracker.Tests;

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
                                    UserId u => "something",
                                    _ => "nothing"
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
    public async Task Negative()
    {
        MakeService().createPasswordUser(new("a", "b"));
        var t = MakeService().createToken(new("a", "b"));
        Assert.NotNull(MakeService().validateToken(t));
        using var h = makeHost();
        var c = h.GetTestClient();
        Assert.Equal(System.Net.HttpStatusCode.OK, (await c.GetAsync("/noauth")).StatusCode);
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, (await c.GetAsync("/auth")).StatusCode);
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("meow!");
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, (await c.GetAsync("/auth")).StatusCode);
        c.UseToken("meow!");
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, (await c.GetAsync("/auth")).StatusCode);
    }

    [Fact]
    public async Task Positive()
    {
        MakeService().createPasswordUser(new("meow", "b"));
        var t = MakeService().createToken(new("meow", "b"));
        Assert.NotNull(MakeService().validateToken(t));
        using var h = makeHost();
        var c = h.GetTestClient();
        c.UseToken(t);
        var correct = await c.GetAsync("/auth");
        Assert.Equal(System.Net.HttpStatusCode.OK, correct.StatusCode);
        Assert.Equal("something", await correct.Content.ReadAsStringAsync());
    }
}
