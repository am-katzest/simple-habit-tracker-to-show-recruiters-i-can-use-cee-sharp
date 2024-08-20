using System.ComponentModel.DataAnnotations;

namespace HabitTracker.DTOs;

public record CompletionTypeData([RegularExpression("^#[0-9a-fA-F]{6}$")][Required] string Color, string Name, string? Description);

public record CompletionTypeDataId(string Color, string Name, string? Description, int Id);

public record CompletionTypeId(int Id, HabitId Habit);


public enum ColorReplacementStrategy
{
    NeverReplace,
    AlwaysReplace,
    ReplaceOnlyIfNotSet
}

// it would be so much easier if c# supported enums with values
public record CompletionTypeRemovalStrategy(String? Note, ColorReplacementStrategy ColorStrategy, [Required] bool DeleteCompletions) : IValidatableObject
{
    IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
    {
        var otherFieldsNotEmpty = ColorStrategy is not ColorReplacementStrategy.NeverReplace || Note is not null;
        if (DeleteCompletions && otherFieldsNotEmpty)
        {
            yield return new ValidationResult($"when {nameof(DeleteCompletions)} is set other fields should not demand changes");
        }
    }
}
