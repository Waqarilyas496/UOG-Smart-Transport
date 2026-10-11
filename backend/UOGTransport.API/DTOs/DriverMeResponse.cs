namespace UOGTransport.API.DTOs;

public class DriverMeResponse
{
    public Guid DriverId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public Guid? BusId { get; set; }
    public string? BusNumber { get; set; }
    public Guid? RouteId { get; set; }
    public string? RouteName { get; set; }
    public Guid? ActiveTripId { get; set; }
}