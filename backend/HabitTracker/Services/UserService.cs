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
    Task<UserId> createPasswordUser(Credentials lp);
    Task<string> createToken(Credentials lp);
    Task removeToken(string token);
    Task deleteUser(UserId user);
    Task<UserId> validateToken(string token);
    Task<AccountDetails> GetAccountDetails(UserId user);
    Task clearExpiredTokens();
};

public class UserService(HabitTrackerContext context, IClock clock) : IUserService
{
    public async Task<UserId> createPasswordUser(Credentials cred)
    {
        var auth = new LoginPassword { Username = cred.Login, Password = cred.Password };
        var u = new User { DisplayName = cred.Login, Auth = auth };
        using var t = await context.Database.BeginTransactionAsync();
        if (await context.GetUserByUsername(cred.Login) is null)
        {
            context.Add(u);
            await context.SaveChangesAsync();
            await t.CommitAsync();
            return new(u.Id);
        }
        else
        {
            throw new DuplicateUsernameException();
        }
    }

    private async Task<string> CreateToken(User user)
    {
        var t = new SessionToken { ExpirationDate = clock.Now.AddDays(2), User = user };
        context.Add(t);
        await context.SaveChangesAsync();
        return t.Id;
    }

    async Task<string> IUserService.createToken(Credentials cred)
    {
        var user = await context.GetUserByUsername(cred.Login);
        if (user is not null)
        {
            if (user.Auth is LoginPassword lp)
            {
                if (lp.Password.Equals(cred.Password))
                {
                    return await CreateToken(user);
                }
            }
        }
        throw new InvalidUsernameOrPasswordException();
    }

    async Task IUserService.deleteUser(UserId user)
    {
        context.Remove(await GetUser(user));
        await context.SaveChangesAsync();
    }

    async Task IUserService.removeToken(string token)
    {
        await context.Tokens.Where(t => t.Id == token).ExecuteDeleteAsync();
    }

    private bool IsValid(Token t) => t.ExpirationDate > clock.Now;
    async Task<UserId> IUserService.validateToken(string token)
    {
        try
        {
            var t = await context.Tokens.SingleAsync(t => t.Id == token);
            if (IsValid(t) && t is SessionToken st)
            {
                await context.Entry(st).Reference(st => st.User).LoadAsync(); // unnecessary roundtrip
                // actually both are unnecessary...
                return new(st.User.Id);
            }
            throw new InvalidTokenException();
        }
        catch (Exception)
        {
            throw new InvalidTokenException();
        }
    }

    async Task IUserService.clearExpiredTokens()
    {
        await context.Tokens.Where(x => x.ExpirationDate > clock.Now).ExecuteDeleteAsync();
    }

    private async Task<User> GetUser(UserId user)
    {
        return await context.Users.SingleAsync(u => u.Id == user.Id);
    }

    async Task<AccountDetails> IUserService.GetAccountDetails(UserId user)
    {
        return new(user.Id, (await GetUser(user)).DisplayName);
    }
}
