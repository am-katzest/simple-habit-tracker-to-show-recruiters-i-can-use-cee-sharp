using HabitTracker.DTOs;
using HabitTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HabitTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(IUserService Service) : ControllerBase
{
    [HttpPost("Create")]
    public ActionResult<int> CreateUserLoginPassword(UserLoginPassword lp)
    {
        var u = Service.createPasswordUser(lp.Login, lp.Password);
        return new(u.Id);
    }

    [HttpPost("CreateToken")]
    public ActionResult<string> GetAuthToken(UserLoginPassword lp)
    {
        return new(Service.createToken(lp.Login, lp.Password));
    }
    // mostly for testing
    [Authorize]
    [HttpGet("id")]
    public ActionResult<int> GetUserId([ModelBinder] UserIdOnly user)
    {
        return new(user.id);
    }
}
