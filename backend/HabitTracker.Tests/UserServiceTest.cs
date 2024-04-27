using HabitTracker.Services;
using HabitTracker.Tests;
using Microsoft.EntityFrameworkCore;

public class UserServiceTest(CreatedDatabaseFixture Fixture) : IClassFixture<CreatedDatabaseFixture>
{
    private IUserService MakeService() => new UserService(Fixture.MakeContext(), new RealClock());

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
}
