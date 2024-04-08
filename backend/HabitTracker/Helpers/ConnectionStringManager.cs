using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.SqlServer;

namespace HabitTracker;

public static class ConnectionStringManager
{
    public record Details
    (
        string host,
        string user,
        string password,
        string db
    );

    private static string GetEnv(string v)
    {
        var s = System.Environment.GetEnvironmentVariable(v);
        if (s is null || s == "")
        {
            throw new System.Exception($"ENVVAR {v} not set");
        }
        return s;
    }

    public static Details GetConnectionDetailsFromEnvironment()
    {

        string host = GetEnv("SHT_DB_HOST");
        string user = GetEnv("SHT_DB_USER");
        string password = GetEnv("SHT_DB_PASSWORD");
        return new Details(host, user, password, "NHT");
    }

    private static Details? connection_override = null; // global, but only changed for testing, and only once

    public static void OverrideConnectionDetails(int port, string user, string password, string db)
    {
        connection_override = new Details($"localhost:{port}", user, password, db);
    }

    private static Details SelectConnectionDetails()
    {
        return connection_override ?? GetConnectionDetailsFromEnvironment();
    }

    public static string GetConnectionString()
    {
        return GenerateDBConnectionString(SelectConnectionDetails());
    }

    private static string GenerateDBConnectionString(Details conn)
    {
        return $"Host={conn.host}; Database={conn.db}; Username={conn.user};Password={conn.password}";
    }
}
