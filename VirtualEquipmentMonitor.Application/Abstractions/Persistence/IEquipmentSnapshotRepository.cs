using VirtualEquipmentMonitor.Domain.Equipment;

namespace VirtualEquipmentMonitor.Application.Abstractions.Persistence;
public interface IEquipmentSnapshotRepository
{
    Task AddAsync(
        EquipmentSnapshot snapshot,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<EquipmentSnapshot>> GetRecentAsync(
        string deviceId,
        int count,
        CancellationToken cancellationToken);
}
