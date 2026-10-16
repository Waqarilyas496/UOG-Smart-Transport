namespace UOGTransport.API.DTOs;

public class IssuePassRequest
{
    public Guid StudentId { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidUntil { get; set; }
}

public class PassResponse
{
    public Guid Id { get; set; }
    public string PassNumber { get; set; } = string.Empty;
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string RollNumber { get; set; } = string.Empty;
    public Guid RouteId { get; set; }
    public string RouteName { get; set; } = string.Empty;
    public DateTime ValidFrom { get; set; }
    public DateTime ValidUntil { get; set; }
    public string Status { get; set; } = string.Empty;
}