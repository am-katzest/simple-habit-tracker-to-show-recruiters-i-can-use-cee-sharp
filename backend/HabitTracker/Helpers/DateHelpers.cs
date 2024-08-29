using HabitTracker.DTOs;

namespace HabitTracker.Helpers;

public static class DateHelpers
{
    public static DateTime Normalized(this DateTime date)
    {
        return DateTime.SpecifyKind(date.ToUniversalTime(), DateTimeKind.Unspecified);
    }
    public static CompletionData WithNormalizedDate(this CompletionData comp)
    {
        return comp with { CompletionDate = comp.CompletionDate.Normalized() };
    }
}
