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
        var u = new User { DisplayName = "u1", Auth = new LoginPassword { Username = "awawa", Password = "" } };
        _ctx.Users.Add(u);
        _ctx.SaveChanges();
        Assert.Equal(1, Diff);
    }

    [Fact]
    public void InsertedUserIsntChanged()
    {
        var auth = new LoginPassword { Username = "uniq1", Password = "some password" };
        var u = new User { DisplayName = "u2", Auth = auth };
        _ctx.Users.Add(u);
        _ctx.SaveChanges();
        var returned = _ctx.Users.Single(u => u.DisplayName == "u2");
        auth = (LoginPassword)returned.Auth;
        Assert.True(auth.Username.Equals("uniq1"));
        Assert.False(auth.Password.Equals("nope, wrong"));
        Assert.True(auth.Password.Equals("some password"));
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
