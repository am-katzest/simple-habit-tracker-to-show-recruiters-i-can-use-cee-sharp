namespace HabitTracker.DTOs.User;

public record Credentials(string Login, string Password);

public record IdOnly(int Id);

public record AccountDetails(int Id, string DisplayName);
