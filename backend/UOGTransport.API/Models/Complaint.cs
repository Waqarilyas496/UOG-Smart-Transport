using UOGTransport.API.Enums;

namespace UOGTransport.API.Models;

public class Complaint
{
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }
    public Student? Student { get; set; }

    public Guid? TripId { get; set; }
    public Trip? Trip { get; set; }

    public ComplaintType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public CaseStatus Status { get; set; } = CaseStatus.Pending;

    public Guid? AssignedToUserId { get; set; }
    public User? AssignedToUser { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
}