namespace UOGTransport.API.DTOs;

public class StartTripRequest
{
    public Guid BusId { get; set; }
    public Guid RouteId { get; set; }
}