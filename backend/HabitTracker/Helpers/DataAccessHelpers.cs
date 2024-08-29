using HabitTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Helpers;
public static class DataAccessExtensionMethods
{
    public static async Task<User?> GetUserByUsername(this HabitTrackerContext ctx, String username)
    {
        try
        {
            return await ctx.Users.Include(u => u.Auth).SingleAsync(u => (u.Auth is LoginPassword) && ((LoginPassword)u.Auth).Username == username);
        }
        catch (InvalidOperationException)
        {
            return null;
        }

    }
}
