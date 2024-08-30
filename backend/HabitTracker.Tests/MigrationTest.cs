using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Tests;

public class MigrationTest(CreatedDatabaseFixture fixture) : IClassFixture<CreatedDatabaseFixture>
{
    [Fact]
    public void DidNotForgetToCreateMigration()
    {
        Assert.False(fixture.MakeContext().Database.HasPendingModelChanges());
    }
}
