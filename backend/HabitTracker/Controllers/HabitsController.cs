using HabitTracker.DTOs;
using HabitTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HabitTracker.Controllers.HabitsController;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class HabitsController(IHabitService Service) : ControllerBase
{
    [HttpPost("")]
    public ActionResult<int> CreateHabit([FromBody] HabitNameDescription habit, [FromHeader][ModelBinder] UserId user)
    {
        return new(Service.addHabit(habit, user).Id);
    }
    [HttpGet("")]
    public ActionResult<List<HabitNameId>> getHabits([ModelBinder] UserId user)
    {
        return new(Service.getHabits(user));
    }
    [HttpGet("{Id:int}")]
    public ActionResult<HabitNameDescriptionId> getHabitDetails([ModelBinder] UserId user, [FromRoute] int id)
    {
        return new(Service.getHabitDetails(new(id, user)));
    }
    [HttpDelete("{Id:int}")]
    public void deleteHabit([ModelBinder] UserId user, [FromRoute] int id)
    {
        Service.RemoveHabit(new(id, user));
    }
    [HttpPut("{Id:int}")]
    public void updateHabit([ModelBinder][FromHeader] UserId user, [FromRoute] int id, [FromBody] HabitNameDescription habit)
    {
        Service.UpdateHabit(new(id, user), habit);
    }
}
