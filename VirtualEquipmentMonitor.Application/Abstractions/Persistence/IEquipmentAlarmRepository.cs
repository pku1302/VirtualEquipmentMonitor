using System;
using System.Collections.Generic;
using System.Text;
using VirtualEquipmentMonitor.Domain.Alarms;

namespace VirtualEquipmentMonitor.Application.Abstractions.Persistence;
public interface IEquipmentAlarmRepository
{
    Task AddRangeAsync(
        IReadOnlyCollection<EquipmentAlarm> alarms,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<EquipmentAlarm>> GetRecentAsync(
        string deviceId,
        int count,
        CancellationToken cancellationToken);
}
