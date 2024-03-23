// isolate tests by giving them different databases on the same postgres container
// this will backfire terribly if more than one test runs at the same time !!!

namespace Database.Tests;


public class DbFixture
{
    private static DbFixture? _instance;
    public static DbFixture Instance
    {
        get
        {
            if (_instance == null)
                _instance = new DbFixture();
            return _instance;
        }
        set { _instance = value; }
    }

    private readonly DbContainer _container;
    protected DbFixture()
    {
        _container = new DbContainer();
    }
    public void UseUniqueDatabase(){
        string rand = System.Guid.NewGuid().ToString();
        _container.OverrideDefault(rand);
    }
}
