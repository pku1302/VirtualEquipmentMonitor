using System;
using System.Collections.Generic;
using System.Text;
using VirtualEquipmentMonitor.Application.Abstractions;
using VirtualEquipmentMonitor.Domain.Equipment;

namespace VirtualEquipmentMonitor.Desktop.Tests.Fakes;
public sealed class FakeEquipmentHistoryService
    : IEquipmentHistoryService
{
    private readonly IReadOnlyList<EquipmentSnapshot> _snapshots;

    public FakeEquipmentHistoryService(
        IReadOnlyList<EquipmentSnapshot>? snapshots = null)
    {
        _snapshots = snapshots ?? [];
    }

    public Task<IReadOnlyList<EquipmentSnapshot>> GetRecentAsync(
        string deviceId,
        int count,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<EquipmentSnapshot> result =
            _snapshots
                .Where(snapshot =>
                    snapshot.DeviceId == deviceId)
                .OrderByDescending(snapshot =>
                    snapshot.Timestamp)
                .Take(count)
                .ToList();

        return Task.FromResult(result);
    }
}
