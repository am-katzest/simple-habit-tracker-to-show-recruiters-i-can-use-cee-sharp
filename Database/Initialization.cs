using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.SqlServer;

namespace Database;

public static class Initialization
{
    public record Details
    {
        public string host;
        public string user;
        public string password;

        public Details(string host, string user, string password)
        {
            this.host = host;
            this.user = user;
            this.password = password;
        }
    }

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
        return new Details(host, user, password);
    }

    private static Details? connection_override = null; // global, but only changed for testing, and only once

    public static void OverrideConnectionDetails(int port, string user, string password)
    {
        if (connection_override is null)
        {
            connection_override = new Details($"localhost:{port}", user, password);
        }
        else
        {
            throw new System.Exception("connection was overriden before (testing broke)");
        }
    }

    private static Details SelectConnectionDetails()
    {
        return connection_override ?? GetConnectionDetailsFromEnvironment();
    }

    public static string GenerateDBConnectionString(string db)
    {
        return GenerateDBConnectionString(db, SelectConnectionDetails());
    }

    static string GenerateDBConnectionString(string db, Details conn)
    {
        return $"Host={conn.host}; Database={db}; Username={conn.user};Password={conn.password}";
    }
}