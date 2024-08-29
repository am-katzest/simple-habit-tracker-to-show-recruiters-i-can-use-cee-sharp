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
    public async Task<ActionResult<int>> CreateHabit([FromHeader][ModelBinder] UserId user, HabitNameDescription habit)
    {
        return new((await service.addHabit(habit, user)).Id);
    }
    [HttpGet("")]
    public async Task<ActionResult<List<HabitNameId>>> getHabits([ModelBinder] UserId user)
    {
        return new(await service.getHabits(user));
    }
    [HttpGet("{Id:int}")]
    public async Task<ActionResult<HabitNameDescriptionId>> getHabitDetails([ModelBinder] UserId user, int id)
    {
        return new(await service.getHabitDetails(new(id, user)));
    }
    [HttpDelete("{Id:int}")]
    public async Task<IActionResult> deleteHabit([ModelBinder] UserId user, int id)
    {
        await service.RemoveHabit(new(id, user));
        return NoContent();
    }
    [HttpPut("{Id:int}")]
    public async Task<IActionResult> updateHabit([ModelBinder][FromHeader] UserId user, int id, HabitNameDescription habit)
    {
        await service.UpdateHabit(new(id, user), habit);
        return NoContent();
    }

    [HttpPost("{habitId:int}/CompletionTypes")]
    public async Task<ActionResult<int>> CreateCompletionType([ModelBinder][FromHeader] UserId user, int habitId, CompletionTypeData ct)
    {
        return new((await service.AddCompletionType(new(habitId, user), ct)).Id);
    }

    [HttpPut("{habitId:int}/CompletionTypes/{completionTypeId:int}")]
    public async Task<IActionResult> UpdateCompletionType([ModelBinder][FromHeader] UserId user, int habitId, int completionTypeId, CompletionTypeData ct)
    {
        var id = new CompletionTypeId(completionTypeId, new(habitId, user));
        await service.UpdateCompletionType(id, ct);
        return NoContent();
    }

    [HttpDelete("{habitId:int}/CompletionTypes/{completionTypeId:int}")]
    public async Task<IActionResult> DeleteCompletionType([ModelBinder][FromHeader] UserId user, int habitId, int completionTypeId, [FromQuery] CompletionTypeRemovalStrategy s, [FromQuery] bool? deleteCompletions = null)
    {
        var id = new CompletionTypeId(completionTypeId, new(habitId, user));
        if (deleteCompletions is null) //should be (s is null), but complex types are always bound to default values
        {
            await service.RemoveCompletionType(id);
        }
        else
        {
            await service.RemoveCompletionType(id, s);
        }
        return NoContent();
    }

    [HttpGet("{habitId:int}/CompletionTypes/")]
    public async Task<ActionResult<List<CompletionTypeDataId>>> GetCompletionTypes([ModelBinder][FromHeader] UserId user, int habitId)
    {
        return new(await service.GetCompletionTypes(new(habitId, user)));
    }

    [HttpPost("{habitId:int}/Completions/")]
    public async Task<ActionResult<int>> PostCompletion([ModelBinder][FromHeader] UserId user, int habitId, CompletionData data)
    {
        var typedHabitId = new HabitId(habitId, user);
        return new((await service.AddCompletion(typedHabitId, data.WithNormalizedDate())).Id);
    }

    [HttpPut("{habitId:int}/Completions/{completionId:int}")]
    public async Task<IActionResult> UpdateCompletion([ModelBinder][FromHeader] UserId user, int habitId, int completionId, CompletionData data)
    {
        var typedHabitId = new HabitId(habitId, user);
        var typedCompletionId = new CompletionId(completionId, typedHabitId);
        await service.UpdateCompletion(typedCompletionId, data.WithNormalizedDate());
        return NoContent();
    }

    [HttpDelete("{habitId:int}/Completions/{completionId:int}")]
    public async Task<IActionResult> DeleteCompletion([ModelBinder][FromHeader] UserId user, int habitId, int completionId)
    {
        var typedHabitId = new HabitId(habitId, user);
        var typedCompletionId = new CompletionId(completionId, typedHabitId);
        await service.RemoveCompletion(typedCompletionId);
        return NoContent();
    }

    [HttpGet("{habitId:int}/Completions/")]
    public async Task<ActionResult<List<CompletionDataId>>> GetCompletions([ModelBinder][FromHeader] UserId user, int habitId, [FromQuery] DateTime? after, [FromQuery] DateTime? before, [FromQuery] int? limit)
    {
        var typedHabitId = new HabitId(habitId, user);
        return new(await service.GetCompletions(typedHabitId, before?.Normalized(), after?.Normalized(), limit));
    }
}
