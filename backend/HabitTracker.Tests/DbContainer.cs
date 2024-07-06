using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
namespace HabitTracker.Tests
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
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(POSTGRESS_PORT).UntilCommandIsCompleted("pg_isready"))
            .WithReuse(true)
            .Build();
        }

        public void Start()
        {
            Task.Run(() => container.StartAsync()).Wait();
        }

        public Helpers.ConnectionDetails RawDetails { get => new($"localhost:{container.GetMappedPublicPort(POSTGRESS_PORT)}", "postgres", pgpassword, null!); }
    }
}
