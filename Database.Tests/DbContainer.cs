using Database;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
namespace Database.Tests
{
    public sealed class DbContainer
    {
        private const int POSTGRESS_PORT = 5432;
        private readonly IContainer container;
        private readonly string pgpassword = "awawa";
        public DbContainer()
        {
            container = MakeContainer();
            Start();
        }

        private IContainer MakeContainer()
        {
            return new ContainerBuilder()
            .WithImage("postgres:16.2")
            .WithPortBinding(POSTGRESS_PORT, true)
            .WithEnvironment("POSTGRES_PASSWORD", pgpassword)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(POSTGRESS_PORT))
            .Build();
        }

        public void Start()
        {
            Task.Run(() => container.StartAsync()).Wait();
        }

        public void OverrideDefault(string db_name)
        {
            var port = container.GetMappedPublicPort(POSTGRESS_PORT);
            Database.Initialization.OverrideConnectionDetails(port, "postgres", pgpassword, db_name);
        }
    }
}