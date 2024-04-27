using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Models;

public class HabitTrackerContext(String connectionString) : DbContext
{
    public DbSet<User> Users { get; set; } = null!;

    public DbSet<Token> Tokens { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // only way to guarantee strict 1:1 user - userAuth relationship with polymorphism (which i know of)
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

        modelBuilder.Entity<Token>()
            .HasDiscriminator<string>("type")
            .HasValue<SessionToken>("session");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseNpgsql(connectionString);
}
