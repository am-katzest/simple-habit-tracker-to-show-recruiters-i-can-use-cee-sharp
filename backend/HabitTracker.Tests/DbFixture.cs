using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;

namespace HabitTracker.Tests;


public class ContainerHolder
{
    private static DbContainer? _instance;
    private static readonly object _mutex = new();
    public static DbContainer Container
    {
        get
        {
            lock (_mutex)
            {
                _instance ??= new DbContainer();
                return _instance;
            }
        }
        set { _instance = value; }
    }
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