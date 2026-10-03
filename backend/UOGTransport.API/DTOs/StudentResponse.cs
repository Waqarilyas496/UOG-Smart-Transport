namespace UOGTransport.API.DTOs;

public class StudentResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string RollNumber { get; set; } = string.Empty;
    public string Batch { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public int Semester { get; set; }
    public Guid? RouteId { get; set; }
    public string? RouteName { get; set; }
}