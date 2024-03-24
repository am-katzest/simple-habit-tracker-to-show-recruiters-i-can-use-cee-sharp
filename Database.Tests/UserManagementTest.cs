using Docker.DotNet.Models;
using Microsoft.EntityFrameworkCore;

namespace Database.Tests;

[Collection("unique database")]
public class UserManagementTest
{
    private UserContext _ctx;
    public UserManagementTest()
    {
        _ctx = new UserContext();
        _ctx.Database.EnsureCreated();
    }
    private int _saved = 0;
    private void reset() => _saved = _ctx.Users.Count();
    private int Diff { get { return _ctx.Users.Count() - _saved; } }

    // should catch model being generated in a different way than i think it does
    [Fact]
    public void CanInsertUser()
    {
        reset();
        var u = new User { DisplayName = "u1", Auth = new LoginPassword { Username = "awawa", Password = "", Salt = "" } };
        _ctx.Users.Add(u);
        _ctx.SaveChanges();
        Assert.Equal(1, Diff);
    }
    [Fact]
    public void CannotInsertInvalidUser()
    {
        reset();
        var u = new User { DisplayName = "u1", Auth = null! };
        _ctx.Users.Add(u);
        Assert.Throws<DbUpdateException>(() => _ctx.SaveChanges());
        Assert.Equal(0, Diff);
    }
}