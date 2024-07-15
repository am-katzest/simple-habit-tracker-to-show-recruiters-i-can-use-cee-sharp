using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Tests;
public class UserManagementTest : IClassFixture<UniqueDatabaseFixture>
{
    private readonly HabitTrackerContext _ctx;

    public UserManagementTest(UniqueDatabaseFixture fixture)
    {
        _ctx = fixture.MakeContext();
        _ctx.Database.EnsureCreated();
    }
    private int _saved;
    private void reset()
    {
        _saved = _ctx.Users.Count();
    }

    private int Diff => _ctx.Users.Count() - _saved;

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
    public void PasswordSaltingWorks()
    {
        var a1 = new LoginPassword { Username = "uniq1", Password = "some password" };
        var a2 = new LoginPassword { Username = "uniq2", Password = "some password" };
        Assert.NotEqual(a1.Hash, a2.Hash);
        Assert.NotEqual(a1.Salt, a2.Salt);
    }
    [Fact]
    public void InsertedUserIsntChanged()
    {

        var auth = new LoginPassword { Username = "uniq1", Password = "some password" };
        var u = new User { DisplayName = "u2", Auth = auth };
        _ctx.Users.Add(u);
        _ctx.SaveChanges();
        var returned = _ctx.Users.Single(u => u.DisplayName == "u2");
        var auth2 = (LoginPassword)returned.Auth;
        Assert.Equal("uniq1", auth2.Username);
        Assert.False(auth2.Password.Equals("nope, wrong"));
        Assert.True(auth2.Password.Equals("some password"));

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
