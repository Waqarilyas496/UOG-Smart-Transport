using UOGTransport.API.Enums;

namespace UOGTransport.API.DTOs;

public class ScanRequest
{
    public string PassNumber { get; set; } = string.Empty;
    public PassengerEventType Type { get; set; }
}

public class ScanResponse
{
    public string StudentName { get; set; } = string.Empty;
    public string RollNumber { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime ScannedAt { get; set; }
    public int OnBoard { get; set; }
    public int Capacity { get; set; }
}

public class OccupancyResponse
{
    public Guid TripId { get; set; }
    public int OnBoard { get; set; }
    public int Capacity { get; set; }
}