namespace UOGTransport.API.DTOs;

public class LiveBusResponse
{
    public bool HasRoute { get; set; }
    public string? RouteName { get; set; }
    public bool HasActiveTrip { get; set; }
    public Guid? TripId { get; set; }
    public string? BusNumber { get; set; }
    public string? DriverName { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public decimal? Speed { get; set; }
    public DateTime? RecordedAt { get; set; }
}