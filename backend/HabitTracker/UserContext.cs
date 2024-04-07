using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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

public class DebugAuth : UserAuth
{
    public required int Key { get; set; }  // to identify users during tests
}

public class LoginPassword : UserAuth
{
    public required string Username { get; init; }
    [StringLength(64)]
    public string Hash { get; private set; } = null!;
    [StringLength(40)]
    public string Salt { get; private set; } = null!;

    // >:3
    // i don't know where joke stops and encapsulation begins
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
        return Convert.ToHexString(SHA256.HashData(input));
    }

    private void storePassword(IEquatable<string> value)
    {
        Salt = RandomNumberGenerator.GetHexString(40);
        Hash = CalculateHash((string)value);
    }
}


// only way to guarantee strict 1:1 user - userAuth relationship with polymorphism (which i know of)
public class UserContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(uu =>
        {
            uu.ToTable("Users");
            uu.HasOne(u => u.Auth).WithOne()
            .HasForeignKey<UserAuth>(ua => ua.Id);
            uu.Navigation(u => u.Auth).IsRequired();
        });

        modelBuilder.Entity<UserAuth>()
            .ToTable("Users")
            .HasDiscriminator<string>("auth_type")
            .HasValue<LoginPassword>("login_password")
            .HasValue<DebugAuth>("debug_disabled");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseNpgsql(ConnectionStringManager.GetConnectionString());
}
