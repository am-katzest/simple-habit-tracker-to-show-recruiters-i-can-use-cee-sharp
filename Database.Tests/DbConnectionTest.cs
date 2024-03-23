using System.Drawing.Printing;
using System.Threading.Channels;
using System.Threading.Tasks.Dataflow;
using Microsoft.EntityFrameworkCore;

namespace Database.Tests;

public sealed class DbConnectionTest
{
    public DbConnectionTest()
    {
        DbFixture.Instance.UseUniqueDatabase();
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
