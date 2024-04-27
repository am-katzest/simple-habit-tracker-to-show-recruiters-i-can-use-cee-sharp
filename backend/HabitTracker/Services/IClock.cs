namespace HabitTracker.Services;

public interface IClock
{
    public DateTime Now { get; }
}

public class RealClock : IClock
{
    public DateTime Now => DateTime.Now;
}

public class ConstantClock(DateTime time) : IClock
{
    public DateTime Now => time;
}
