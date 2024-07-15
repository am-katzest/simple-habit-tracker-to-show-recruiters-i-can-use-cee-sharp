using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;

namespace HabitTracker.Models;

public class User
{
    public int Id { get; set; }
    public required string DisplayName { get; set; }
    public required UserAuth Auth { get; set; }
    public ICollection<Habit> Habits { get; } = new List<Habit>();
}

public abstract class UserAuth
{
    public int Id { get; set; }
} // i plan on adding oauth2

public class DebugAuth : UserAuth
{
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

