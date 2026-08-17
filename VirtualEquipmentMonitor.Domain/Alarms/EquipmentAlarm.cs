using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualEquipmentMonitor.Domain.Alarms;
public sealed record EquipmentAlarm
{
    public string DeviceId { get; }
    public DateTimeOffset OccuredAt { get; }
    public AlarmType Type { get; }
    public AlarmSeverity Severity { get; }
    public double MeasuredValue { get; }
    public double Threshold { get; }
    public EquipmentAlarm(
        string deviceId,
        DateTimeOffset occuredAt,
        AlarmType type,
        AlarmSeverity severity,
        double measuredValue,
        double threshold)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            throw new ArgumentException(
                "장비 ID는 비어 있을 수 없습니다.",
                nameof(deviceId));
        }

        DeviceId = deviceId;
        OccuredAt = occuredAt;
        Type = type;
        Severity = severity;
        MeasuredValue = measuredValue;
        Threshold = threshold;
    }
}
