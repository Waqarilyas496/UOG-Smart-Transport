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
    public DbSet<Stop> Stops { get; set; }
    public DbSet<Trip> Trips { get; set; }
    public DbSet<GpsLocation> GpsLocations { get; set; }
    public DbSet<PassengerRecord> PassengerRecords { get; set; }
    public DbSet<TransportPass> TransportPasses { get; set; }
    public DbSet<FuelRecord> FuelRecords { get; set; }
    public DbSet<MaintenanceRecord> MaintenanceRecords { get; set; }
    public DbSet<Complaint> Complaints { get; set; }
    public DbSet<EmergencyReport> EmergencyReports { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

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

        modelBuilder.Entity<Trip>().Property(t => t.Status).HasConversion<string>();
        modelBuilder.Entity<PassengerRecord>().Property(p => p.Type).HasConversion<string>();
        modelBuilder.Entity<TransportPass>().Property(p => p.Status).HasConversion<string>();
        modelBuilder.Entity<Complaint>().Property(c => c.Type).HasConversion<string>();
        modelBuilder.Entity<Complaint>().Property(c => c.Status).HasConversion<string>();
        modelBuilder.Entity<EmergencyReport>().Property(e => e.Type).HasConversion<string>();
        modelBuilder.Entity<EmergencyReport>().Property(e => e.Status).HasConversion<string>();
        modelBuilder.Entity<Notification>().Property(n => n.Type).HasConversion<string>();

        // Unique constraints
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<Student>().HasIndex(s => s.RollNumber).IsUnique();
        modelBuilder.Entity<Driver>().HasIndex(d => d.LicenseNumber).IsUnique();
        modelBuilder.Entity<Bus>().HasIndex(b => b.BusNumber).IsUnique();
        modelBuilder.Entity<TransportPass>().HasIndex(p => p.PassNumber).IsUnique();
    }
}