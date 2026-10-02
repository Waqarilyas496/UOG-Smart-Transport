namespace UOGTransport.API.DTOs;

public class DriverResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public Guid? AssignedBusId { get; set; }
    public string? AssignedBusNumber { get; set; }
}