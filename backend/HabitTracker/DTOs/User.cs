namespace HabitTracker.DTOs;

public record Credentials(string Login, string Password);

public record UserId(int Id);

public record AccountDetails(int Id, string DisplayName);
