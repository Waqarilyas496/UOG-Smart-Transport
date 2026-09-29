using UOGTransport.API.Enums;

namespace UOGTransport.API.Models;

public class EmergencyReport
{
    public Guid Id { get; set; }

    public Guid? TripId { get; set; }
    public Trip? Trip { get; set; }

    public Guid ReportedByUserId { get; set; }
    public User? ReportedByUser { get; set; }

    public EmergencyType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public CaseStatus Status { get; set; } = CaseStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}