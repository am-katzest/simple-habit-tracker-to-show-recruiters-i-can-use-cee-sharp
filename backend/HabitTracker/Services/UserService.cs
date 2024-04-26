//i know that Identity exists, and by writing this myself
// i only introduce vurneabilities and that it's a bad idea
// yet i don't care :3 (it's not like anyone but me will ever use it)

using HabitTracker.Models;

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
        throw new NotImplementedException();
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
