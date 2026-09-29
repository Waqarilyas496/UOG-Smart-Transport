using UOGTransport.API.Enums;

namespace UOGTransport.API.Models;

public class TransportPass
{
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }
    public Student? Student { get; set; }

    public Guid RouteId { get; set; }
    public BusRoute? Route { get; set; }

    public string PassNumber { get; set; } = string.Empty;
    public DateTime ValidFrom { get; set; }
    public DateTime ValidUntil { get; set; }
    public PassStatus Status { get; set; } = PassStatus.Active;
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
}