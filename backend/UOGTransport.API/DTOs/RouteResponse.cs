namespace UOGTransport.API.DTOs;

public class RouteResponse
{
    public Guid Id { get; set; }
    public string RouteName { get; set; } = string.Empty;
    public string StartPoint { get; set; } = string.Empty;
    public string EndPoint { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}