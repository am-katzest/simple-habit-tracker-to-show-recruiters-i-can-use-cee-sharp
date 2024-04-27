//i know that Identity exists, and by writing this myself
// i only introduce vurneabilities and that it's a bad idea
// yet i don't care :3 (it's not like anyone but me will ever use it)

using HabitTracker.Helpers;
using HabitTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Services;

public interface IUserService
{
    User createPasswordUser(string username, string password);
    string createToken(string username, string password);
    void removeToken(string token);
    void deleteUser(User user);
    User validateToken(string token);
};

public class UserService(HabitTrackerContext Context, IClock Clock) : IUserService
{
    public User createPasswordUser(string username, string password)
    {
        var auth = new LoginPassword { Username = username, Password = password };
        var u = new User { DisplayName = username, Auth = auth };
        Context.Database.BeginTransaction();
        if (Context.GetUserByUsername(username) is null)
        {
            Context.Add(u);
            Context.SaveChanges();
            Context.Database.CommitTransaction();
            return u;
        }
        else
        {
            Context.Database.RollbackTransaction();
            throw new DuplicateUsernameException();
        }
    }

    private string CreateToken(User User)
    {
        var t = new SessionToken { ExpirationDate = Clock.Now.AddMinutes(20), User = User };
        Context.Add(t);
        Context.SaveChanges();
        return t.Id;
    }

    string IUserService.createToken(string username, string password)
    {
        var user = Context.GetUserByUsername(username);
        if (user is not null)
        {
            if (user.Auth is LoginPassword lp)
            {
                if (lp.Password.Equals(password))
                {
                    return CreateToken(user);
                }
            }
        }
        throw new InvalidUsernameOrPasswordException();
    }

    void IUserService.deleteUser(User user)
    {
        Context.Remove(user);
        Context.SaveChanges();
    }

    void IUserService.removeToken(string token)
    {
        Context.Tokens.Where(t => t.Id == token).ExecuteDelete();
    }

    private bool IsValid(Token t) => t.ExpirationDate > Clock.Now;
    User IUserService.validateToken(string token)
    {
        try
        {
            var t = Context.Tokens.Single(t => t.Id == token);
            if (IsValid(t) && t is SessionToken st)
            {
                Context.Entry(st).Reference(st => st.User).Load(); // another roundtrip for no good reason, on the hottest of paths
                return st.User;
            }
            throw new InvalidTokenException();
        }
        catch (Exception)
        {
            throw new InvalidTokenException();
        }
    }
}
