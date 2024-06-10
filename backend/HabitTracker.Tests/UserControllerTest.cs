using System.Net.Http.Headers;
using HabitTracker.DTOs;
using Microsoft.AspNetCore.TestHost;
using static System.Net.HttpStatusCode;

namespace HabitTracker.Tests;
public class UserControllerTest(HostFixture fixture) : IClassFixture<HostFixture>
{
    [Fact]
    public async void UserCreationPositive()
    {
        using var h = fixture.makeHost();
        using var c = h.GetTestClient();
        var lp = new UserLoginPassword() { Login = "kitty", Password = "cat" };
        var ans1 = await c.PostAsJsonAsync("api/user/create", lp);
        Assert.Equal(OK, ans1.StatusCode);
    }

    [Fact]
    public async void AuthorizationNegative()
    {
        using var h = fixture.makeHost();
        using var c = h.GetTestClient();
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("awawa");
        var ans1 = await c.GetAsync("api/user/displayname");
        Assert.NotEqual(OK, ans1.StatusCode);
    }
    [Fact]
    public async void AuthorizationPositive()
    {
        using var h = fixture.makeHost();
        using var c = h.GetTestClient();
        var lp = new UserLoginPassword() { Login = "user1", Password = "cat" };
        var ans1 = await c.PostAsJsonAsync("api/user/create", lp);
        Assert.Equal(OK, ans1.StatusCode);
        Assert.True(await c.AuthenticateUser(lp));
        var ans2 = await c.GetAsync("api/user/id");
        Assert.Equal(OK, ans2.StatusCode);
        var id1 = await ans1.Content.ReadAsStringAsync();
        var id2 = await ans2.Content.ReadAsStringAsync();
        Assert.Equal(id1, id2);
        Assert.NotEqual("", id2);
    }
}
