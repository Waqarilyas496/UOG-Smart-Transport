using UOGTransport.API.Enums;

namespace UOGTransport.API.Models;

public class Bus
{
    public Guid Id { get; set; }
    public string BusNumber { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public BusStatus Status { get; set; } = BusStatus.Active;

    // Foreign key to BusRoute (nullable: a bus may not be assigned a route yet)
    public Guid? RouteId { get; set; }
    public BusRoute? Route { get; set; }
}