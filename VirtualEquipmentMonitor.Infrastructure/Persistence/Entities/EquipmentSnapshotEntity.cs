namespace VirtualEquipmentMonitor.Application.Abstractions.Persistence.Entities;
public sealed class EquipmentSnapshotEntity
{
    public long Id { get; set;}
    public string DeviceId { get; set; } = string.Empty;
    public DateTime TimestampUtc { get; set; }
    public string State { get; set; } = string.Empty;
    public double Temperature { get; set; }
    public int Rpm { get; set; }
    public double Vibration { get; set; }
}