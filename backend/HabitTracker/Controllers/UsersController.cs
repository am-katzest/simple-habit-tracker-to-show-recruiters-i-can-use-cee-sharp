using HabitTracker.DTOs.User;
using HabitTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HabitTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(IUserService Service) : ControllerBase
{
    [HttpPost("")]
    public ActionResult<int> CreateUserLoginPassword(Credentials cred)
    {
        return new(Service.createPasswordUser(cred).Id);
    }

    [HttpPost("CreateToken")]
    public ActionResult<string> GetAuthToken(Credentials cred)
    {
        return new(Service.createToken(cred));
    }

    [Authorize]
    [HttpGet("me")]
    public ActionResult<AccountDetails> GetUserData([ModelBinder] IdOnly user)
    {
        return new(Service.GetAccountDetails(user));
    }
    [Authorize]
    [HttpDelete("me")]
    public ActionResult<bool> DeleteUser([ModelBinder] IdOnly user)
    {
        Service.deleteUser(user);
        return new(true);
    }
}