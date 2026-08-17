using VirtualEquipmentMonitor.Application.Abstractions;
using VirtualEquipmentMonitor.Application.Abstractions.Persistence;
using VirtualEquipmentMonitor.Domain.Equipment;

namespace VirtualEquipmentMonitor.Application.Services;

public sealed class EquipmentHistoryService
    : IEquipmentHistoryService
{
    private readonly IEquipmentSnapshotRepository _repository;

    public EquipmentHistoryService(
        IEquipmentSnapshotRepository repository)
    {
        _repository = repository;
    }
    public Task<IReadOnlyList<EquipmentSnapshot>> GetRecentAsync(
        string deviceId,
        int count,
        CancellationToken cancellationToken)
    {
        return _repository.GetRecentAsync(
            deviceId,
            count,
            cancellationToken);
    }
}
