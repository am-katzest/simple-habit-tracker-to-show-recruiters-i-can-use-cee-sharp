namespace HabitTracker.Tests;

public static class OtherExtensionMethods
{
    // https://stackoverflow.com/questions/152774/is-there-a-better-way-to-trim-a-datetime-to-a-specific-precision
    public static DateTime TrimToMinute(this DateTime date)
    {
        return new DateTime(date.Ticks - date.Ticks % TimeSpan.TicksPerMinute, date.Kind);
    }
}
