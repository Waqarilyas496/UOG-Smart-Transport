namespace UOGTransport.API.DTOs;

public class DriverRequest
{
    public string LicenseNumber { get; set; } = string.Empty;
    public Guid? AssignedBusId { get; set; }
}