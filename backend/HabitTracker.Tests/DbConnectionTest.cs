namespace HabitTracker.Tests;

[Collection("unique database")]
public sealed class DbConnectionTest
{
    [Fact]
    public void DbStartsAndIsEmpty()
    {
        var ctx = new HabitTrackerContext();
        Assert.Throws<Npgsql.PostgresException>(() => ctx.Users.Count());
        ctx.Database.EnsureCreated();
        Assert.Equal(0, ctx.Users.Count());
    }
}
