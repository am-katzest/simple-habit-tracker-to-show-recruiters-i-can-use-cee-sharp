using Microsoft.EntityFrameworkCore;

namespace Database;

public class User
{
    public int Id { get; set; }
    public required string DisplayName { get; set; }
    public required UserAuth Auth { get; set; }
}

public abstract class UserAuth
{
    public int Id { get; set; }
} // i plan on adding oauth2

public class LoginPassword : UserAuth
{
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string Salt { get; set; }
}


public class UserContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserAuth>()
            .HasDiscriminator<string>("auth_type")
            .HasValue<LoginPassword>("login_password");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseNpgsql(ConnectionStringManager.GetConnectionString());
}