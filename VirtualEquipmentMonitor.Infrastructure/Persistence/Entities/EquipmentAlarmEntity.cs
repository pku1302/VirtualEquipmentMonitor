
namespace VirtualEquipmentMonitor.Infrastructure.Persistence.Entities;

public sealed class EquipmentAlarmEntity
{
    public long Id { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public DateTime OccurredAtUtc { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public double MeasuredValue { get; set; }
    public double Threshold { get; set; }
}
