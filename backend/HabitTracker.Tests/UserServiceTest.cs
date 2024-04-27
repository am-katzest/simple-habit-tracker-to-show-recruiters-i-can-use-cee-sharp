using HabitTracker.Services;
using HabitTracker.Tests;
using Microsoft.EntityFrameworkCore;

public class UserServiceTest(CreatedDatabaseFixture Fixture) : IClassFixture<CreatedDatabaseFixture>
{
    private IUserService MakeService() => MakeService(new RealClock());
    private IUserService MakeService(IClock clock) => new UserService(Fixture.MakeContext(), clock);

    [Fact]
    public void UserCreationWorks()
    {
        MakeService().createPasswordUser("kitty", "password");
        var u = Fixture.MakeContext().Users.Include(u => u.Auth).Single(u => u.DisplayName == "kitty");
        Assert.NotNull(u);
        Assert.Equal("kitty", u.DisplayName);
        var a = u.Auth;
        if (a is LoginPassword lp)
        {
            Assert.Equal("kitty", lp.Username);
            Assert.True(lp.Password.Equals("password"));
        }
        else
        {
            Assert.IsType<LoginPassword>(a);
        }
    }

    [Fact]
    public void InsertingDuplicateUser()
    {
        MakeService().createPasswordUser("awawa", "passwd");
        Assert.Throws<DuplicateUsernameException>(() => MakeService().createPasswordUser("awawa", "passwd"));
    }

    [Fact]
    public void TokenCreationTest()
    {
        var t1 = MakeService().createToken("kitty", "password");
        var t2 = MakeService().createToken("kitty", "password");
        Assert.NotEqual(t1, t2);
    }

    [Fact]
    public void TokenCreationInvalid()
    {
        Assert.Throws<InvalidUsernameOrPasswordException>(() => MakeService().createToken("awawa", "blah"));
        Assert.Throws<InvalidUsernameOrPasswordException>(() => MakeService().createToken("kitty", "blah"));
    }

    [Fact]
    public void TokenValidationPositive()
    {
        var t1 = MakeService().createToken("kitty", "password");
        var u = MakeService().validateToken(t1);
        Assert.Equal("kitty", u.DisplayName);
    }

    [Fact]
    public void TokenValidationNegative()
    {
        var t1 = MakeService().createToken("kitty", "password");
        Assert.Throws<InvalidTokenException>(() => MakeService().validateToken(""));
        Assert.Throws<InvalidTokenException>(() => MakeService().validateToken(t1 + "a"));
        Assert.Throws<InvalidTokenException>(() => MakeService(new ConstantClock(DateTime.Now.AddYears(1))).validateToken(t1));
    }

    [Fact]
    public void TokenDeletionTest()
    {
        MakeService().createPasswordUser("kittyy", "password");
        var t1 = MakeService().createToken("kittyy", "password");
        var t2 = MakeService().createToken("kittyy", "password");
        MakeService().removeToken(t2);
        Assert.NotNull(MakeService().validateToken(t1));
        Assert.Throws<InvalidTokenException>(() => MakeService().validateToken(t2));
    }

    [Fact]
    public void UserDeletionTest()
    {
        var s = MakeService();
        s.createPasswordUser("a", "b");
        var t = s.createToken("a", "b");
        var t2 = s.createToken("a", "b");
        s.deleteUser(s.validateToken(t));
        Assert.Throws<InvalidTokenException>(() => s.validateToken(t));
        Assert.Throws<InvalidTokenException>(() => s.validateToken(t2));
    }

    [Fact]
    public void TokenClearingTest()
    {
        var time1 = new ConstantClock(DateTime.Now.AddDays(531));
        var time11 = new ConstantClock(DateTime.Now.AddMinutes(5));
        var time2 = new ConstantClock(DateTime.Now.AddDays(532));

        MakeService(time1).clearExpiredTokens();
        // from other tests
        var leftovers = Fixture.MakeContext().Tokens.Count();
        MakeService(time1).createPasswordUser("b", "c");
        var t1 = MakeService(time1).createToken("b", "c");
        var t11 = MakeService(time11).createToken("b", "c");
        var t2 = MakeService(time2).createToken("b", "c");
        MakeService(time2).clearExpiredTokens();
        Assert.Equal(2, Fixture.MakeContext().Tokens.Count() - leftovers);
    }
}
