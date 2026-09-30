namespace UOGTransport.API.DTOs;

public class BusResponse
{
    public Guid Id { get; set; }
    public string BusNumber { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? RouteId { get; set; }
    public string? RouteName { get; set; }
}