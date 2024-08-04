namespace HabitTracker.DTOs;

public record CompletionId(int Id, HabitId Habit);

public record CompletionData(int? CompletionTypeId, DateTime CompletionDate, string? Note, string? Color);

public record CompletionDataId(int Id, int? CompletionTypeId, DateTime CompletionDate, string? Note, string? Color) :
    CompletionData(CompletionTypeId, CompletionDate, Note, Color);
