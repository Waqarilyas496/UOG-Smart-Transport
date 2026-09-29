using Microsoft.EntityFrameworkCore;
using UOGTransport.API.Models;

namespace UOGTransport.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<Driver> Drivers { get; set; }
    public DbSet<Bus> Buses { get; set; }
    public DbSet<BusRoute> Routes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Table names (BusRoute class -> "Routes" table, matches our DATABASE.md doc)
        modelBuilder.Entity<BusRoute>().ToTable("Routes");

        // Enums stored as text in the database (readable in pgAdmin, e.g. "Student" not "3")
        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<string>();

        modelBuilder.Entity<Bus>()
            .Property(b => b.Status)
            .HasConversion<string>();

        // Unique constraints
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<Student>().HasIndex(s => s.RollNumber).IsUnique();
        modelBuilder.Entity<Driver>().HasIndex(d => d.LicenseNumber).IsUnique();
        modelBuilder.Entity<Bus>().HasIndex(b => b.BusNumber).IsUnique();
    }
}