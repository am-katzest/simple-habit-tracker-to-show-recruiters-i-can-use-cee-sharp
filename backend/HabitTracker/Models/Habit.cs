using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace HabitTracker.Models;

public class Habit
{
    public int Id { get; set; }
    public required User User { get; set; }
    public required int UserId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required ICollection<Completion> Completions { get; set; }
    public required ICollection<CompletionType> CompletionTypes { get; set; }
}
