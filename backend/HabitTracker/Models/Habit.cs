namespace HabitTracker.Models;

public class Habit
{
    public int Id { get; set; }
    public User User { get; set; } = null!;
    public required int UserId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public List<Completion> Completions { get; set; } = null!;
    public List<CompletionType> CompletionTypes { get; set; } = null!;
}
