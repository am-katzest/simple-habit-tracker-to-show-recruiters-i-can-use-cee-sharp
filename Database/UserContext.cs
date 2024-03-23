using Microsoft.EntityFrameworkCore;

namespace Database;

public class Kitty {
    public int KittyId {get; set;}
}

public class UserContext : DbContext
{
    public DbSet<Kitty> Kitties { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseNpgsql(ConnectionStringManager.GetConnectionString());
}