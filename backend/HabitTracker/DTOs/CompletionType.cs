using System.ComponentModel.DataAnnotations;

namespace HabitTracker.DTOs;

public record CompletionTypeData([RegularExpression("^#[0-9a-fA-F]{6}$")][Required] string Color, string Name, string? Description);

public record CompletionTypeDataId(string Color, string Name, string? Description, int Id);

public record CompletionTypeId(int Id, HabitId Habit);
