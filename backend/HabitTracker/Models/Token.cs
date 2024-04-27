using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace HabitTracker.Models;

public class Token
{
    [StringLength(40)]
    public required String Id { get; set; } = RandomNumberGenerator.GetHexString(40);

    public required DateTime ExpirationDate { get; set; }
}

public class SessionToken : Token
{
    public required User User { get; set; }
}
