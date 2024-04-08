// isolate tests by giving them different databases on the same postgres container
// this will backfire terribly if more than one test runs at the same time !!!

namespace HabitTracker.Tests;


public class ContainerHolder
{
    private static ContainerHolder? _instance;
    public static ContainerHolder Instance
    {
        get
        {
            _instance ??= new ContainerHolder();
            return _instance;
        }
        set { _instance = value; }
    }

    private readonly DbContainer _container;
    protected ContainerHolder()
    {
        _container = new DbContainer();
    }
    public void UseUniqueDatabase()
    {
        string rand = System.Guid.NewGuid().ToString();
        string date = System.DateTime.Now.ToString("o");
        _container.OverrideDefault(date + rand);
    }
}


public sealed class UniqueDatabaseFixture : IDisposable
{
    public UniqueDatabaseFixture()
    {
        ContainerHolder.Instance.UseUniqueDatabase();
    }
    public void Dispose() { }
}

[CollectionDefinition("unique database", DisableParallelization = true)]
public class UniqueDatabaseCollection : ICollectionFixture<UniqueDatabaseFixture> { }
