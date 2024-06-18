using System.Net.Http.Headers;
using HabitTracker.DTOs.User;
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
        var lp = new Credentials("kitty", "cat");
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
        var lp = new Credentials("user1", "cat");
        var ans1 = await c.PostAsJsonAsync("api/user/create", lp);
        Assert.Equal(OK, ans1.StatusCode);
        Assert.True(await c.AuthenticateUser(lp));
        var ans2 = await c.GetFromJsonAsync<AccountDetails>("api/user/me");
        var id1 = await ans1.Content.ReadAsStringAsync();
        Assert.NotNull(ans2);
        Assert.Equal(id1, ans2!.Id.ToString());
    }
    [Fact]
    public async void DeletionPositive()
    {
        using var h = fixture.makeHost();
        using var c = h.GetTestClient();
        var lp = new Credentials("user2", "cat");
        Assert.True(await c.RegisterUser(lp));
        var ans = await c.DeleteAsync("/api/user/me");
        Assert.True(ans.IsSuccessStatusCode);
        Assert.False((await c.GetAsync("/api/users/me")).IsSuccessStatusCode);
        //Assert.False(await c.AuthenticateUser(lp)); TODO
    }
}
