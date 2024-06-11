using HabitTracker.DTOs.User;
using HabitTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HabitTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(IUserService Service) : ControllerBase
{
    [HttpPost("Create")]
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
}
