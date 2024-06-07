using System.Net;
using HabitTracker.DTOs;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json.Serialization;
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
}
