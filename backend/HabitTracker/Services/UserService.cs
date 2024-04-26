//i know that Identity exists, and by writing this myself
// i only introduce vurneabilities and that it's a bad idea
// yet i don't care :3 (it's not like anyone but me will ever use it)

using HabitTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Services;

public interface IUserService
{
    User createPasswordUser(string username, string password);
    string createToken(string username, string password);
    void removeToken(string token);
    void deleteUser(string username);
    User validateToken(string token);
};

public class UserService(HabitTrackerContext Context) : IUserService
{
    public User createPasswordUser(string username, string password)
    {
        var auth = new LoginPassword { Username = username, Password = password };
        var u = new User { DisplayName = username, Auth = auth};
        Context.Database.BeginTransaction();
        // c# doesn't let me do this correctly ;-;
        var existing = Context.Users.Include(u => u.Auth).Count(u => (u.Auth is LoginPassword) && ((LoginPassword)u.Auth).Username == username);
        if (existing == 0) {
            Context.Add(u);
            Context.SaveChanges();
            Context.Database.CommitTransaction();
            return u;
        } else {
            Context.Database.RollbackTransaction();
            throw new DuplicateUsernameException();
        }
    }

    string IUserService.createToken(string username, string password)
    {
        throw new NotImplementedException();
    }

    void IUserService.deleteUser(string username)
    {
        throw new NotImplementedException();
    }

    void IUserService.removeToken(string token)
    {
        throw new NotImplementedException();
    }

    User IUserService.validateToken(string token)
    {
        throw new NotImplementedException();
    }
}
