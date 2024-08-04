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

public class UserService(HabitTrackerContext context, IClock clock) : IUserService
{
    public UserId createPasswordUser(Credentials cred)
    {
        var auth = new LoginPassword { Username = cred.Login, Password = cred.Password };
        var u = new User { DisplayName = cred.Login, Auth = auth };
        context.Database.BeginTransaction();
        if (context.GetUserByUsername(cred.Login) is null)
        {
            context.Add(u);
            context.SaveChanges();
            context.Database.CommitTransaction();
            return new(u.Id);
        }
        else
        {
            context.Database.RollbackTransaction();
            throw new DuplicateUsernameException();
        }
    }

    private string CreateToken(User user)
    {
        var t = new SessionToken { ExpirationDate = clock.Now.AddDays(2), User = user };
        context.Add(t);
        context.SaveChanges();
        return t.Id;
    }

    string IUserService.createToken(Credentials cred)
    {
        var user = context.GetUserByUsername(cred.Login);
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
        context.Remove(GetUser(user));
        context.SaveChanges();
    }

    void IUserService.removeToken(string token)
    {
        context.Tokens.Where(t => t.Id == token).ExecuteDelete();
    }

    private bool IsValid(Token t) => t.ExpirationDate > clock.Now;
    UserId IUserService.validateToken(string token)
    {
        try
        {
            var t = context.Tokens.Single(t => t.Id == token);
            if (IsValid(t) && t is SessionToken st)
            {
                context.Entry(st).Reference(st => st.User).Load(); // unnecessary roundtrip
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
        context.Tokens.Where(x => x.ExpirationDate > clock.Now).ExecuteDelete();
    }

    private User GetUser(UserId user) => context.Users.Single(u => u.Id == user.Id);
    AccountDetails IUserService.GetAccountDetails(UserId user)
    {
        return new(user.Id, GetUser(user).DisplayName);
    }
}
