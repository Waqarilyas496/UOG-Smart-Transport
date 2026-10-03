namespace UOGTransport.API.DTOs;

public class StudentRequest
{
    public string RollNumber { get; set; } = string.Empty;
    public string Batch { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public int Semester { get; set; }
    public Guid? RouteId { get; set; }
}