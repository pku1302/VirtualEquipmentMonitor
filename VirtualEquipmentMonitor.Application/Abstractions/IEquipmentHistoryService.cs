using System;
using System.Collections.Generic;
using System.Text;
using VirtualEquipmentMonitor.Domain.Equipment;

namespace VirtualEquipmentMonitor.Application.Abstractions;

public interface IEquipmentHistoryService
{
    Task<IReadOnlyList<EquipmentSnapshot>> GetRecentAsync(
        string deviceId,
        int count,
        CancellationToken cancellationToken);
}
