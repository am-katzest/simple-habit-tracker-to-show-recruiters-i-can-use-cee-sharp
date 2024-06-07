using HabitTracker.DTOs;
using HabitTracker.Models;
using HabitTracker.Services;
using Microsoft.AspNetCore.Mvc;

namespace HabitTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(IUserService Service) : ControllerBase
{
    [HttpPost("Create")]
    public ActionResult<string> CreateUserLoginPassword(UserLoginPassword lp)
    {
        Service.createPasswordUser(lp.Login, lp.Password);
        return new(Service.createToken(lp.Login, lp.Password));
    }

    [HttpGet("AuthToken")]
    public ActionResult<string> GetAuthToken(UserLoginPassword lp)
    {
        return new(Service.createToken(lp.Login, lp.Password));
    }

}
