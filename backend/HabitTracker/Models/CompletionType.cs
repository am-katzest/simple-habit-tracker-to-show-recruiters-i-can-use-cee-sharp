namespace HabitTracker.Models;

public class CompletionType
{
    public int Id { get; set; }
    public required Habit Habit { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required string Color { get; set; }
}
