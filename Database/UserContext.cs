using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;
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
    public required string Username { get; init; }
    private string Hash { get; set; } = null!;
    private string Salt { get; set; } = null!;

    // >:3
    [NotMapped]
    public required IEquatable<string> Password
    {
        set { storePassword(value); }
        get { return new HashEqualityCheckerAdapter { Checker = PasswordMatches }; }
    }

    public bool PasswordMatches(string? pass) => pass != null && Hash == CalculateHash(pass);

    private class HashEqualityCheckerAdapter : IEquatable<string>
    {
        public required Predicate<string?> Checker { get; init; }
        public bool Equals(string? password) => Checker(password);
    }

    private string CalculateHash(string val)
    {
        //  from https://stackoverflow.com/questions/16999361/obtain-sha-256-string-of-a-string
        byte[] input = Encoding.UTF8.GetBytes(val + Salt);
        var byteArray = System.Security.Cryptography.SHA256.HashData(input);
        return Convert.ToHexString(byteArray);
    }

    private void storePassword(IEquatable<string> value)
    {
        Salt = System.Guid.NewGuid().ToString();
        Hash = CalculateHash((string)value);
    }
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
