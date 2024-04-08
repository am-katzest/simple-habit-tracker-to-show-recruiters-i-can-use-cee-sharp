namespace HabitTracker.Tests;

public sealed class DbConnectionTest(UniqueDatabaseFixture fixture) : IClassFixture<UniqueDatabaseFixture>
{
    [Fact]
    public void DbStartsAndIsEmpty()
    {
        var ctx = fixture.MakeContext();
        Assert.Throws<Npgsql.PostgresException>(() => ctx.Users.Count());
        ctx.Database.EnsureCreated();
        Assert.Equal(0, ctx.Users.Count());
    }
}
