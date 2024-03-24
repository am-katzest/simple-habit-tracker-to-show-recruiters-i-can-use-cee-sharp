// isolate tests by giving them different databases on the same postgres container
// this will backfire terribly if more than one test runs at the same time !!!

namespace Database.Tests;


public class ContainerHolder
{
    private static ContainerHolder? _instance;
    private static readonly Object _init_lock = new();
    public static ContainerHolder Instance
    {
        get
        {

            if (_instance == null)
                _instance = new ContainerHolder();
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
        _container.OverrideDefault(rand);
    }
}


public class UniqueDatabaseFixture : IDisposable
{
    public UniqueDatabaseFixture()
    {
        ContainerHolder.Instance.UseUniqueDatabase();
    }
    public void Dispose() { }
}

[CollectionDefinition("unique database", DisableParallelization = true)]
public class UniqueDatabaseCollection : ICollectionFixture<UniqueDatabaseFixture> { }