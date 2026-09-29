namespace UOGTransport.API.Models;

public class Student
{
    public Guid Id { get; set; }

    // Link to base User account
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public string RollNumber { get; set; } = string.Empty;
    public string Batch { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public int Semester { get; set; }

    // Assigned route (nullable: may not be assigned yet)
    public Guid? RouteId { get; set; }
    public BusRoute? Route { get; set; }
}