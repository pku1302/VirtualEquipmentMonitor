
using VirtualEquipmentMonitor.Domain.Equipment;

namespace VirtualEquipmentMonitor.Desktop.Models;
public sealed record EquipmentHistoryItem(
    string DeviceId,
    DateTimeOffset Timestamp,
    string State,
    double Temperature,
    int Rpm,
    double Vibration)
{
    public DateTimeOffset LocalTimestamp =>
        Timestamp.ToLocalTime();

    public static EquipmentHistoryItem FromDomain(
        EquipmentSnapshot snapshot)
    {
        return new EquipmentHistoryItem(
            snapshot.DeviceId,
            snapshot.Timestamp,
            snapshot.State.ToString(),
            snapshot.Temperature,
            snapshot.Rpm,
            snapshot.Vibration);
    }
}
