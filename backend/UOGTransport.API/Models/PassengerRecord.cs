using UOGTransport.API.Enums;

namespace UOGTransport.API.Models;

public class PassengerRecord
{
    public Guid Id { get; set; }

    public Guid TripId { get; set; }
    public Trip? Trip { get; set; }

    public Guid StudentId { get; set; }
    public Student? Student { get; set; }

    public PassengerEventType Type { get; set; }
    public DateTime ScannedAt { get; set; } = DateTime.UtcNow;

    public Guid ScannedByDriverId { get; set; }
    public Driver? ScannedByDriver { get; set; }
}