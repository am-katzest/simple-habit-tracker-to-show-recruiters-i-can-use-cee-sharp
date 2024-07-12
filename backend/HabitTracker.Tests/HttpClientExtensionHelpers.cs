using System.Net.Http.Headers;

namespace HabitTracker.Tests;

public static class HttpClientExtensionMethods
{
    public static async Task<bool> AuthenticateUser(this HttpClient c, DTOs.User.Credentials credentials)
    {
        var ans = await c.PostAsJsonAsync("api/users/createtoken", credentials);
        if (!ans.IsSuccessStatusCode)
        {
            return false;
        }
        var token = await ans.Content.ReadAsStringAsync();
        c.UseToken(token);
        return true;
    }

    public static void UseToken(this HttpClient c, string token)
    {
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("SessionToken", token);
    }
    public static async Task<bool> RegisterUser(this HttpClient c, DTOs.User.Credentials credentials)
    {
        var ans = await c.PostAsJsonAsync("api/users", credentials);
        if (!ans.IsSuccessStatusCode)
        {
            return false;
        }
        return await c.AuthenticateUser(credentials);
    }

    static int idseq = 0;
    public static async Task<bool> RegisterNewUniqueUser(this HttpClient c)
    {
        var u = Interlocked.Increment(ref idseq);
        return await c.RegisterUser(new($"unique_user_{u}", "password123"));
    }
}
