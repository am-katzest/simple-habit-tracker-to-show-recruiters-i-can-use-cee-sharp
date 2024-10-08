using System.ComponentModel.DataAnnotations.Schema;

namespace HabitTracker.Models;

public class Completion
{
    public int Id { get; set; }
    public required Habit Habit { get; set; }
    public int? TypeId { get; set; }
    public CompletionType? Type { get; set; }
    public string? Note { get; set; }
    public string? Color { get; set; }

    [Column(TypeName = "timestamp(6)")]
    public DateTime CompletionDate { get; set; }
    public bool IsExactTime { get; set; }
}
