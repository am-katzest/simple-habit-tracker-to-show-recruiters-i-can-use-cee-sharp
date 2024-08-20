using HabitTracker.DTOs;
using HabitTracker.Helpers;
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
    public IActionResult DeleteCompletionType([ModelBinder][FromHeader] UserId user, int habitId, int completionTypeId, [FromQuery] CompletionTypeRemovalStrategy s, [FromQuery] bool? deleteCompletions = null)
    {
        var id = new CompletionTypeId(completionTypeId, new(habitId, user));
        if (deleteCompletions is null) //should be (s is null), but complex types are always bound to default values
        {
            service.RemoveCompletionType(id);
        }
        else
        {
            service.RemoveCompletionType(id, s);
        }
        return NoContent();
    }

    [HttpGet("{habitId:int}/CompletionTypes/")]
    public ActionResult<List<CompletionTypeDataId>> GetCompletionTypes([ModelBinder][FromHeader] UserId user, int habitId)
    {
        return new(service.GetCompletionTypes(new(habitId, user)));
    }

    [HttpPost("{habitId:int}/Completions/")]
    public ActionResult<int> PostCompletion([ModelBinder][FromHeader] UserId user, int habitId, CompletionData data)
    {
        var TypedHabitId = new HabitId(habitId, user);
        return new(service.AddCompletion(TypedHabitId, data.WithNormalizedDate()).Id);
    }

    [HttpPut("{habitId:int}/Completions/{completionId:int}")]
    public IActionResult UpdateCompletion([ModelBinder][FromHeader] UserId user, int habitId, int completionId, CompletionData data)
    {
        var TypedHabitId = new HabitId(habitId, user);
        var TypedCompletionId = new CompletionId(completionId, TypedHabitId);
        service.UpdateCompletion(TypedCompletionId, data.WithNormalizedDate());
        return NoContent();
    }

    [HttpDelete("{habitId:int}/Completions/{completionId:int}")]
    public IActionResult DeleteCompletion([ModelBinder][FromHeader] UserId user, int habitId, int completionId)
    {
        var TypedHabitId = new HabitId(habitId, user);
        var TypedCompletionId = new CompletionId(completionId, TypedHabitId);
        service.RemoveCompletion(TypedCompletionId);
        return NoContent();
    }

    [HttpGet("{habitId:int}/Completions/")]
    public ActionResult<List<CompletionDataId>> GetCompletions([ModelBinder][FromHeader] UserId user, int habitId, [FromQuery] DateTime? after, [FromQuery] DateTime? before, [FromQuery] int? limit)
    {
        var TypedHabitId = new HabitId(habitId, user);
        return new(service.GetCompletions(TypedHabitId, before?.Normalized(), after?.Normalized(), limit));
    }
}
