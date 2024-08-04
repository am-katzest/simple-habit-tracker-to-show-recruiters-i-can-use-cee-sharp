using HabitTracker.DTOs;
using HabitTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HabitTracker.Controllers.UsersController;

[ApiController]
[Route("api/[controller]")]
public class UsersController(IUserService service) : ControllerBase
{
    [HttpPost("")]
    public ActionResult<int> CreateUserLoginPassword(Credentials cred)
    {
        return new(service.createPasswordUser(cred).Id);
    }

    [HttpPost("CreateToken")]
    public ActionResult<string> GetAuthToken(Credentials cred)
    {
        return new(service.createToken(cred));
    }

    [Authorize]
    [HttpGet("me")]
    public ActionResult<AccountDetails> GetUserData([ModelBinder] UserId user)
    {
        return new(service.GetAccountDetails(user));
    }
    [Authorize]
    [HttpDelete("me")]
    public ActionResult<bool> DeleteUser([ModelBinder] UserId user)
    {
        service.deleteUser(user);
        return new(true);
    }
}
