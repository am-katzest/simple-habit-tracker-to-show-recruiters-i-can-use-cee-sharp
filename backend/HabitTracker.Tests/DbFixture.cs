using HabitTracker.DTOs;

namespace HabitTracker.Tests;


public class ContainerHolder
{
    private static Lazy<DbContainer> _instance = new(() => new());

    public static DbContainer Container { get => _instance.Value; }
}


public class UniqueDatabaseFixture : IDisposable
{
    private readonly string _conn_string;
    public UniqueDatabaseFixture()
    {
        string rand = System.Guid.NewGuid().ToString();
        string date = System.DateTime.Now.ToString("o");
        string db_name = date + rand;
        var details = ContainerHolder.Container.RawDetails with { db = db_name };
        _conn_string = details.Format();

    }

    public HabitTrackerContext MakeContext() => new(_conn_string);
    public void Dispose() { }
}

public class CreatedDatabaseFixture : UniqueDatabaseFixture
{
    public CreatedDatabaseFixture() : base()
    {
        MakeContext().Database.EnsureCreated();
    }
}

public class UserFixture : CreatedDatabaseFixture
{
    public UserId MakeUser()
    {
        var c = MakeContext();
        var u = new User() { DisplayName = "user", Auth = new DebugAuth() };
        c.Users.Add(u);
        c.SaveChanges();
        return new(u.Id);
    }
}
