namespace UOGTransport.API.Models;

public class MaintenanceRecord
{
    public Guid Id { get; set; }

    public Guid BusId { get; set; }
    public Bus? Bus { get; set; }

    public string ServiceType { get; set; } = string.Empty;
    public DateTime ServiceDate { get; set; }
    public decimal? Cost { get; set; }
    public DateTime? NextServiceDue { get; set; }
    public string? PerformedBy { get; set; }
    public string? Notes { get; set; }
}