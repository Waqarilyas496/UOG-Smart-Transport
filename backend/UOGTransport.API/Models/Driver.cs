namespace UOGTransport.API.Models;

public class Driver
{
    public Guid Id { get; set; }

    // Link to base User account
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public string LicenseNumber { get; set; } = string.Empty;

    // Currently assigned bus (nullable: may not be assigned yet)
    public Guid? AssignedBusId { get; set; }
    public Bus? AssignedBus { get; set; }
}