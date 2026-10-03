namespace UOGTransport.API.DTOs;

public class TripResponse
{
    public Guid Id { get; set; }
    public Guid BusId { get; set; }
    public string BusNumber { get; set; } = string.Empty;
    public Guid DriverId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public Guid RouteId { get; set; }
    public string RouteName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
}