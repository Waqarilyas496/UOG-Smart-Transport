namespace UOGTransport.API.Models;

public class GpsLocation
{
    public Guid Id { get; set; }

    public Guid TripId { get; set; }
    public Trip? Trip { get; set; }

    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public decimal? Speed { get; set; }
    public decimal? Heading { get; set; }

    public DateTime RecordedAt { get; set; }
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}