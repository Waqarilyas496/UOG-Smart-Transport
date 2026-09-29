using UOGTransport.API.Enums;

namespace UOGTransport.API.Models;

public class Trip
{
    public Guid Id { get; set; }

    public Guid BusId { get; set; }
    public Bus? Bus { get; set; }

    public Guid DriverId { get; set; }
    public Driver? Driver { get; set; }

    public Guid RouteId { get; set; }
    public BusRoute? Route { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public TripStatus Status { get; set; } = TripStatus.Scheduled;
}