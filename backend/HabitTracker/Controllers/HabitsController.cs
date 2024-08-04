using HabitTracker.DTOs;
using HabitTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HabitTracker.Controllers.HabitsController;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class HabitsController(IHabitService service) : ControllerBase
{
    [HttpPost("")]
    public ActionResult<int> CreateHabit([FromHeader][ModelBinder] UserId user, HabitNameDescription habit)
    {
        return new(service.addHabit(habit, user).Id);
    }
    [HttpGet("")]
    public ActionResult<List<HabitNameId>> getHabits([ModelBinder] UserId user)
    {
        return new(service.getHabits(user));
    }
    [HttpGet("{Id:int}")]
    public ActionResult<HabitNameDescriptionId> getHabitDetails([ModelBinder] UserId user, int id)
    {
        return new(service.getHabitDetails(new(id, user)));
    }
    [HttpDelete("{Id:int}")]
    public IActionResult deleteHabit([ModelBinder] UserId user, int id)
    {
        service.RemoveHabit(new(id, user));
        return NoContent();
    }
    [HttpPut("{Id:int}")]
    public IActionResult updateHabit([ModelBinder][FromHeader] UserId user, int id, HabitNameDescription habit)
    {
        service.UpdateHabit(new(id, user), habit);
        return NoContent();
    }

    [HttpPost("{habitId:int}/CompletionTypes")]
    public ActionResult<int> CreateCompletionType([ModelBinder][FromHeader] UserId user, int habitId, CompletionTypeData ct)
    {
        return new(service.AddCompletionType(new(habitId, user), ct).Id);
    }

    [HttpPut("{habitId:int}/CompletionTypes/{completionTypeId:int}")]
    public IActionResult UpdateCompletionType([ModelBinder][FromHeader] UserId user, int habitId, int completionTypeId, CompletionTypeData ct)
    {
        var id = new CompletionTypeId(completionTypeId, new(habitId, user));
        service.UpdateCompletionType(id, ct);
        return NoContent();
    }

    [HttpDelete("{habitId:int}/CompletionTypes/{completionTypeId:int}")]
    public IActionResult UpdateCompletionType([ModelBinder][FromHeader] UserId user, int habitId, int completionTypeId)
    {
        var id = new CompletionTypeId(completionTypeId, new(habitId, user));
        service.RemoveCompletionType(id);
        return NoContent();
    }

    [HttpGet("{habitId:int}/CompletionTypes/")]
    public ActionResult<List<CompletionTypeDataId>> UpdateCompletionType([ModelBinder][FromHeader] UserId user, int habitId)
    {
        return new(service.GetCompletionTypes(new(habitId, user)));
    }
}
