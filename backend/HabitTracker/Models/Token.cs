using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace HabitTracker.Models;

public class Token
{
    [StringLength(40)]
    public String Id { get; set; } = RandomNumberGenerator.GetHexString(40);

    // https://stackoverflow.com/questions/73693917/net-postgres-ef-core-cannot-write-datetime-with-kind-local-to-postgresql-type/78119521#78119521
    [Column(TypeName = "timestamp(6)")]
    public required DateTime ExpirationDate { get; set; }
}

public class SessionToken : Token
{
    public required User User { get; set; }
}
