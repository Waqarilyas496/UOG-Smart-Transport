namespace UOGTransport.API.Models;

public class Stop
{
    public Guid Id { get; set; }

    public Guid RouteId { get; set; }
    public BusRoute? Route { get; set; }

    public string StopName { get; set; } = string.Empty;
    public int StopOrder { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
}