namespace HabitTracker.DTOs;

public record HabitNameDescription(string Name, string? Description);

public record HabitNameId(string Name, int Id);

public record HabitNameDescriptionId(string Name, int Id, string? Description);

public record HabitId(int Id, UserId User);
