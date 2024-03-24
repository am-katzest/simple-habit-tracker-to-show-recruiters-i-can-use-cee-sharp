namespace Database.Tests;

[Collection("unique database")]
public sealed class DbConnectionTest
{
    [Fact]
    public void DbStartsAndIsEmpty()
    {
        var ctx = new UserContext();
        Assert.Throws<Npgsql.PostgresException>(() => ctx.Kitties.Count());
        ctx.Database.EnsureCreated();
        Assert.Equal(0, ctx.Kitties.Count());
    }
}