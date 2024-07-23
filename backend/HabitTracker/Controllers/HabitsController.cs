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
    public ActionResult<int> CreateHabit([FromHeader][ModelBinder] UserId user, HabitNameDescription habit)
    {
        return new(Service.addHabit(habit, user).Id);
    }
    [HttpGet("")]
    public ActionResult<List<HabitNameId>> getHabits([ModelBinder] UserId user)
    {
        return new(Service.getHabits(user));
    }
    [HttpGet("{Id:int}")]
    public ActionResult<HabitNameDescriptionId> getHabitDetails([ModelBinder] UserId user, int id)
    {
        return new(Service.getHabitDetails(new(id, user)));
    }
    [HttpDelete("{Id:int}")]
    public IActionResult deleteHabit([ModelBinder] UserId user, int id)
    {
        Service.RemoveHabit(new(id, user));
        return NoContent();
    }
    [HttpPut("{Id:int}")]
    public IActionResult updateHabit([ModelBinder][FromHeader] UserId user, int id, HabitNameDescription habit)
    {
        Service.UpdateHabit(new(id, user), habit);
        return NoContent();
    }

    [HttpPost("{Hid:int}/CompletionTypes")]
    public ActionResult<int> CreateCompletionType([ModelBinder][FromHeader] UserId user, int Hid, CompletionTypeData ct)
    {
        return new(Service.AddCompletionType(new(Hid, user), ct).Id);
    }

    [HttpPut("{Hid:int}/CompletionTypes/{Ctid:int}")]
    public IActionResult UpdateCompletionType([ModelBinder][FromHeader] UserId user, int Hid, int Ctid, CompletionTypeData ct)
    {
        var id = new CompletionTypeId(Ctid, new(Hid, user));
        Service.UpdateCompletionType(id, ct);
        return NoContent();
    }

    [HttpDelete("{Hid:int}/CompletionTypes/{Ctid:int}")]
    public IActionResult UpdateCompletionType([ModelBinder][FromHeader] UserId user, int Hid, int Ctid)
    {
        var id = new CompletionTypeId(Ctid, new(Hid, user));
        Service.RemoveCompletionType(id);
        return NoContent();
    }

    [HttpGet("{Hid:int}/CompletionTypes/")]
    public ActionResult<List<CompletionTypeDataId>> UpdateCompletionType([ModelBinder][FromHeader] UserId user, int Hid)
    {
        return new(Service.GetCompletionTypes(new(Hid, user)));
    }
}
