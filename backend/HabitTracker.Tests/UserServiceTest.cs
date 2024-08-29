using HabitTracker.Exceptions;
using HabitTracker.Services;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Tests;

public class UserServiceTest(CreatedDatabaseFixture fixture) : IClassFixture<CreatedDatabaseFixture>
{
    private IUserService MakeService() => MakeService(new RealClock());
    private IUserService MakeService(IClock clock) => new UserService(fixture.MakeContext(), clock);

    [Fact]
    public async Task UserCreationWorks()
    {
        await MakeService().createPasswordUser(new("kitty", "password"));
        var u = fixture.MakeContext().Users.Include(u => u.Auth).Single(u => u.DisplayName == "kitty");
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
    public async Task InsertingDuplicateUser()
    {
        await MakeService().createPasswordUser(new("awawa", "passwd"));
        var count = fixture.MakeContext().Users.Count();
        await Assert.ThrowsAsync<DuplicateUsernameException>(async () => await MakeService().createPasswordUser(new("awawa", "passwd")));
        var after = fixture.MakeContext().Users.Count();
        Assert.Equal(count, after);
    }

    [Fact]
    public async Task TokenCreationTest()
    {
        await MakeService().createPasswordUser(new("e", "f"));
        var t1 = await MakeService().createToken(new("e", "f"));
        var t2 = await MakeService().createToken(new("e", "f"));
        Assert.NotEqual(t1, t2);
    }

    [Fact]
    public async Task TokenCreationInvalid()
    {
        await MakeService().createPasswordUser(new("f", "g"));
        await Assert.ThrowsAsync<InvalidUsernameOrPasswordException>(async () => await MakeService().createToken(new("f", "blah")));
        await Assert.ThrowsAsync<InvalidUsernameOrPasswordException>(async () => await MakeService().createToken(new("not", "blah")));
    }

    [Fact]
    public async Task TokenValidationPositive()
    {

        await MakeService().createPasswordUser(new("d", "e"));
        var t1 = await MakeService().createToken(new("d", "e"));
        var u = await MakeService().validateToken(t1);
        Assert.Equal("d", (await MakeService().GetAccountDetails(u)).DisplayName);
    }

    [Fact]
    public async Task TokenValidationNegative()
    {
        await MakeService().createPasswordUser(new("c", "d"));
        var t1 = await MakeService().createToken(new("c", "d"));
        await Assert.ThrowsAsync<InvalidTokenException>(async () => await MakeService().validateToken(""));
        await Assert.ThrowsAsync<InvalidTokenException>(async () => await MakeService().validateToken(t1 + "a"));
        await Assert.ThrowsAsync<InvalidTokenException>(async () => await MakeService(new ConstantClock(DateTime.Now.AddYears(1))).validateToken(t1));
    }

    [Fact]
    public async Task TokenDeletionTest()
    {
        await MakeService().createPasswordUser(new("kittyy", "password"));
        var t1 = await MakeService().createToken(new("kittyy", "password"));
        var t2 = await MakeService().createToken(new("kittyy", "password"));
        await MakeService().removeToken(t2);
        Assert.NotNull(await MakeService().validateToken(t1));
        await Assert.ThrowsAsync<InvalidTokenException>(async () => await MakeService().validateToken(t2));
    }

    [Fact]
    public async Task UserDeletionTest()
    {
        var s = MakeService();
        await s.createPasswordUser(new("a", "b"));
        var t = await s.createToken(new("a", "b"));
        var t2 = await s.createToken(new("a", "b"));
        await s.deleteUser(await s.validateToken(t));
        await Assert.ThrowsAsync<InvalidTokenException>(async () => await s.validateToken(t));
        await Assert.ThrowsAsync<InvalidTokenException>(async () => await s.validateToken(t2));
    }

    [Fact]
    public async Task TokenClearingTest()
    {
        var time1 = new ConstantClock(DateTime.Now.AddDays(531));
        var time11 = new ConstantClock(DateTime.Now.AddMinutes(5));
        var time2 = new ConstantClock(DateTime.Now.AddDays(535));

        await MakeService(time1).clearExpiredTokens();
        // from other tests
        var leftovers = await fixture.MakeContext().Tokens.CountAsync();
        await MakeService(time1).createPasswordUser(new("b", "c"));
        var t1 = await MakeService(time1).createToken(new("b", "c"));
        var t11 = await MakeService(time11).createToken(new("b", "c"));
        var t2 = await MakeService(time2).createToken(new("b", "c"));
        await MakeService(time2).clearExpiredTokens();
        Assert.Equal(2, (await fixture.MakeContext().Tokens.CountAsync()) - leftovers);
    }
}
