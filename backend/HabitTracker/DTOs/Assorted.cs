namespace HabitTracker.DTOs;
public class UserLoginPassword
{
    public string Login { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public record UserIdOnly(int id);