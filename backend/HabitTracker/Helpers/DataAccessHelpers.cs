using HabitTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Helpers;
public static class DataAccessExtensionMethods
{
    public static User? GetUserByUsername(this HabitTrackerContext ctx, String username)
    {
        try
        {
            return ctx.Users.Include(u => u.Auth).Single(u => (u.Auth is LoginPassword) && ((LoginPassword)u.Auth).Username == username);
        }
        catch (InvalidOperationException)
        {
            return null;
        }

    }
}
