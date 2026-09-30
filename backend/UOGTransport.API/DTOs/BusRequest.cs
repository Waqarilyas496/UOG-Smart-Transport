using UOGTransport.API.Enums;

namespace UOGTransport.API.DTOs;

public class BusRequest
{
    public string BusNumber { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public BusStatus Status { get; set; } = BusStatus.Active;
    public Guid? RouteId { get; set; }
}