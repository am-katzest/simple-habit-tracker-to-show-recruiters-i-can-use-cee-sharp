using System.Net.Http.Headers;
using HabitTracker.DTOs;

namespace HabitTracker.Tests;

public static class HttpClientExtensionMethods
{
    public static async Task<bool> AuthenticateUser(this HttpClient c, UserLoginPassword credentials) {
        var ans = await c.PostAsJsonAsync("api/user/createtoken", credentials);
        if (!ans.IsSuccessStatusCode) {
            return false;
        }
        var token = await ans.Content.ReadAsStringAsync();
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token);
        return true;
    }
}