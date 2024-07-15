using HabitTracker.Exceptions;
using HabitTracker.Services;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Tests;

public class UserServiceTest(CreatedDatabaseFixture Fixture) : IClassFixture<CreatedDatabaseFixture>
{
    private IUserService MakeService() => MakeService(new RealClock());
    private IUserService MakeService(IClock clock) => new UserService(Fixture.MakeContext(), clock);

    [Fact]
    public void UserCreationWorks()
    {
        MakeService().createPasswordUser(new("kitty", "password"));
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
        MakeService().createPasswordUser(new("awawa", "passwd"));
        var count = Fixture.MakeContext().Users.Count();
        Assert.Throws<DuplicateUsernameException>(() => MakeService().createPasswordUser(new("awawa", "passwd")));
        var after = Fixture.MakeContext().Users.Count();
        Assert.Equal(count, after);
    }

    [Fact]
    public void TokenCreationTest()
    {
        MakeService().createPasswordUser(new("e", "f"));
        var t1 = MakeService().createToken(new("e", "f"));
        var t2 = MakeService().createToken(new("e", "f"));
        Assert.NotEqual(t1, t2);
    }

    [Fact]
    public void TokenCreationInvalid()
    {
        MakeService().createPasswordUser(new("f", "g"));
        Assert.Throws<InvalidUsernameOrPasswordException>(() => MakeService().createToken(new("f", "blah")));
        Assert.Throws<InvalidUsernameOrPasswordException>(() => MakeService().createToken(new("not", "blah")));
    }

    [Fact]
    public void TokenValidationPositive()
    {

        MakeService().createPasswordUser(new("d", "e"));
        var t1 = MakeService().createToken(new("d", "e"));
        var u = MakeService().validateToken(t1);
        Assert.Equal("d", MakeService().GetAccountDetails(u).DisplayName);
    }

    [Fact]
    public void TokenValidationNegative()
    {
        MakeService().createPasswordUser(new("c", "d"));
        var t1 = MakeService().createToken(new("c", "d"));
        Assert.Throws<InvalidTokenException>(() => MakeService().validateToken(""));
        Assert.Throws<InvalidTokenException>(() => MakeService().validateToken(t1 + "a"));
        Assert.Throws<InvalidTokenException>(() => MakeService(new ConstantClock(DateTime.Now.AddYears(1))).validateToken(t1));
    }

    [Fact]
    public void TokenDeletionTest()
    {
        MakeService().createPasswordUser(new("kittyy", "password"));
        var t1 = MakeService().createToken(new("kittyy", "password"));
        var t2 = MakeService().createToken(new("kittyy", "password"));
        MakeService().removeToken(t2);
        Assert.NotNull(MakeService().validateToken(t1));
        Assert.Throws<InvalidTokenException>(() => MakeService().validateToken(t2));
    }

    [Fact]
    public void UserDeletionTest()
    {
        var s = MakeService();
        s.createPasswordUser(new("a", "b"));
        var t = s.createToken(new("a", "b"));
        var t2 = s.createToken(new("a", "b"));
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
        MakeService(time1).createPasswordUser(new("b", "c"));
        var t1 = MakeService(time1).createToken(new("b", "c"));
        var t11 = MakeService(time11).createToken(new("b", "c"));
        var t2 = MakeService(time2).createToken(new("b", "c"));
        MakeService(time2).clearExpiredTokens();
        Assert.Equal(2, Fixture.MakeContext().Tokens.Count() - leftovers);
    }
}
