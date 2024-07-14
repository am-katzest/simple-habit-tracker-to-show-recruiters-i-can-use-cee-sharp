//i know that Identity exists, and by writing this myself
// i only introduce vurneabilities and that it's a bad idea
// yet i don't care :3 (it's not like anyone but me will ever use it)

using HabitTracker.DTOs;
using HabitTracker.Exceptions;
using HabitTracker.Helpers;
using HabitTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Services;

public interface IUserService
{
    UserId createPasswordUser(Credentials lp);
    string createToken(Credentials lp);
    void removeToken(string token);
    void deleteUser(UserId user);
    UserId validateToken(string token);
    AccountDetails GetAccountDetails(UserId user);
    void clearExpiredTokens();
};

public class UserService(HabitTrackerContext Context, IClock Clock) : IUserService
{
    public UserId createPasswordUser(Credentials cred)
    {
        var auth = new LoginPassword { Username = cred.Login, Password = cred.Password };
        var u = new User { DisplayName = cred.Login, Auth = auth };
        Context.Database.BeginTransaction();
        if (Context.GetUserByUsername(cred.Login) is null)
        {
            Context.Add(u);
            Context.SaveChanges();
            Context.Database.CommitTransaction();
            return new(u.Id);
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

    string IUserService.createToken(Credentials cred)
    {
        var user = Context.GetUserByUsername(cred.Login);
        if (user is not null)
        {
            if (user.Auth is LoginPassword lp)
            {
                if (lp.Password.Equals(cred.Password))
                {
                    return CreateToken(user);
                }
            }
        }
        throw new InvalidUsernameOrPasswordException();
    }

    void IUserService.deleteUser(UserId user)
    {
        Context.Remove(GetUser(user));
        Context.SaveChanges();
    }

    void IUserService.removeToken(string token)
    {
        Context.Tokens.Where(t => t.Id == token).ExecuteDelete();
    }

    private bool IsValid(Token t) => t.ExpirationDate > Clock.Now;
    UserId IUserService.validateToken(string token)
    {
        try
        {
            var t = Context.Tokens.Single(t => t.Id == token);
            if (IsValid(t) && t is SessionToken st)
            {
                Context.Entry(st).Reference(st => st.User).Load(); // unnecessary roundtrip
                return new(st.User.Id);
            }
            throw new InvalidTokenException();
        }
        catch (Exception)
        {
            throw new InvalidTokenException();
        }
    }

    void IUserService.clearExpiredTokens()
    {
        Context.Tokens.Where(x => x.ExpirationDate > Clock.Now).ExecuteDelete();
    }

    private User GetUser(UserId user) => Context.Users.Single(u => u.Id == user.Id);
    AccountDetails IUserService.GetAccountDetails(UserId user)
    {
        return new(user.Id, GetUser(user).DisplayName);
    }
}
