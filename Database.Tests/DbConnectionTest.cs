using System.Drawing.Printing;
using System.Threading.Channels;
using System.Threading.Tasks.Dataflow;
using Microsoft.EntityFrameworkCore;

namespace Database.Tests;

public sealed class DbConnectionTest : IDisposable
{
    private readonly DBContainerFixture db;
    public DbConnectionTest()
    {
        db = new DBContainerFixture();
    }

    public void Dispose()
    {
        db.Dispose();
    }

    [Fact]
    public void DbStartsAndIsEmpty()
    {
        var ctx = new UserContext();
        Assert.Throws<Npgsql.PostgresException>(() => ctx.Kitties.Count());
        ctx.Database.EnsureCreated();
        Assert.Equal(0, ctx.Kitties.Count());
    }

}
