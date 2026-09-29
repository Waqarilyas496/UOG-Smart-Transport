namespace UOGTransport.API.Models;

public class FuelRecord
{
    public Guid Id { get; set; }

    public Guid BusId { get; set; }
    public Bus? Bus { get; set; }

    public Guid? DriverId { get; set; }
    public Driver? Driver { get; set; }

    public decimal Litres { get; set; }
    public decimal PricePerLitre { get; set; }
    public decimal TotalCost { get; set; }
    public int Odometer { get; set; }
    public string? FuelStation { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
}