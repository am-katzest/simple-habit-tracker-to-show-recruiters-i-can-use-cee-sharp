namespace HabitTracker.Models;

public class Habit
{
    public int Id { get; set; }
    public User User { get; set; } = null!;
    public required int UserId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public ICollection<Completion> Completions { get; } = new List<Completion>();
    public ICollection<CompletionType> CompletionTypes { get; } = new List<CompletionType>();
}
