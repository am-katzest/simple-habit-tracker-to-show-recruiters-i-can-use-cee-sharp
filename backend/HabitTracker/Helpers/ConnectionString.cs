namespace HabitTracker.Helpers;

public record ConnectionDetails
(
    string host,
    string user,
    string password,
    string db
)
{
    public string Format()
    {
        return $"Host={host}; Database={db}; Username={user};Password={password}";
    }

    public static ConnectionDetails FromEnvironment()
    {
        static string GetEnv(string v)
        {
            var s = System.Environment.GetEnvironmentVariable(v);
            if (s is null || s == "")
            {
                throw new System.Exception($"ENVVAR {v} not set");
            }
            return s;
        }
        string host = GetEnv("SHT_DB_HOST");
        string user = GetEnv("SHT_DB_USER");
        string password = GetEnv("SHT_DB_PASSWORD");
        return new ConnectionDetails(host, user, password, "NHT");
    }
}
