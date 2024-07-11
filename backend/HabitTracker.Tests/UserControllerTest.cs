using HabitTracker.DTOs.User;
using HabitTracker.Exceptions;
using static System.Net.HttpStatusCode;

namespace HabitTracker.Tests;
public class UserControllerTest(HostFixture fixture) : IClassFixture<HostFixture>
{
    [Fact]
    public async void UserCreationPositive()
    {
        using var c = fixture.Client;
        var lp = new Credentials("kitty", "cat");
        var ans1 = await c.PostAsJsonAsync("api/users", lp);
        Assert.Equal(OK, ans1.StatusCode);
    }
    [Fact]
    public async void UserCreationNegative()
    {
        using var c = fixture.Client;
        var lp = new Credentials("existing", "cat");
        var ans1 = await c.PostAsJsonAsync("api/users", lp);
        var ans2 = await c.PostAsJsonAsync("api/users", lp);
        Assert.Equal(OK, ans1.StatusCode);
        await AssertHelpers.ReturnedError<DuplicateUsernameException>(ans2);
    }

    [Fact]
    public async void AuthorizationNegative()
    {
        using var c = fixture.Client;
        c.UseToken("awawa");
        var ans1 = await c.GetAsync("api/users/me");
        Assert.NotEqual(OK, ans1.StatusCode);
    }


    [Fact]
    public async void AuthenticationNegative()
    {
        using var c = fixture.Client;
        var credentials = new Credentials("wrong_username", "password");
        var ans = await c.PostAsJsonAsync("api/users/createtoken", credentials);
        await AssertHelpers.ReturnedError<InvalidUsernameOrPasswordException>(ans);
    }

    [Fact]
    public async void AuthorizationPositive()
    {
        using var c = fixture.Client;
        var lp = new Credentials("user1", "cat");
        var ans1 = await c.PostAsJsonAsync("api/users", lp);
        Assert.Equal(OK, ans1.StatusCode);
        Assert.True(await c.AuthenticateUser(lp));
        var ans2 = await c.GetFromJsonAsync<AccountDetails>("api/users/me");
        var id1 = await ans1.Content.ReadAsStringAsync();
        Assert.NotNull(ans2);
        Assert.Equal(id1, ans2!.Id.ToString());
    }
    [Fact]
    public async void DeletionPositive()
    {
        using var c = fixture.Client;
        var lp = new Credentials("user2", "cat");
        Assert.True(await c.RegisterUser(lp));
        var ans = await c.DeleteAsync("/api/users/me");
        Assert.True(ans.IsSuccessStatusCode);
        Assert.False((await c.GetAsync("/api/users/me")).IsSuccessStatusCode);
        Assert.False(await c.AuthenticateUser(lp));
    }
}
