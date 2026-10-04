namespace UOGTransport.API.DTOs;

public class GpsLocationRequest
{
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public decimal? Speed { get; set; }
    public decimal? Heading { get; set; }
    public DateTime RecordedAt { get; set; }
}