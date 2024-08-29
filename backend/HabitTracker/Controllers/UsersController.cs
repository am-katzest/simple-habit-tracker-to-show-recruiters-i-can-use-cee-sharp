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
    public async Task<ActionResult<int>> CreateUserLoginPassword(Credentials cred)
    {
        return new((await service.createPasswordUser(cred)).Id);
    }

    [HttpPost("CreateToken")]
    public async Task<ActionResult<string>> GetAuthToken(Credentials cred)
    {
        return new(await service.createToken(cred));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<AccountDetails>> GetUserData([ModelBinder] UserId user)
    {
        return new(await service.GetAccountDetails(user));
    }
    [Authorize]
    [HttpDelete("me")]
    public async Task<ActionResult<bool>> DeleteUser([ModelBinder] UserId user)
    {
        await service.deleteUser(user);
        return new(true);
    }
}
