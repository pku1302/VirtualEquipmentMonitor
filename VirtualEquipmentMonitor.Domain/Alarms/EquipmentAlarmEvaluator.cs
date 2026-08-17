using System;
using System.Collections.Generic;
using System.Text;
using VirtualEquipmentMonitor.Domain.Equipment;

namespace VirtualEquipmentMonitor.Domain.Alarms;
public sealed class EquipmentAlarmEvaluator
{
    private readonly EquipmentThresholds _thresholds;

    public EquipmentAlarmEvaluator(
        EquipmentThresholds thresholds)
    {
        _thresholds = thresholds;
    }

    public IReadOnlyList<EquipmentAlarm> Evaluate(
        EquipmentSnapshot? previous,
        EquipmentSnapshot current)
    {
        ArgumentNullException.ThrowIfNull(current);

        if (previous is not null &&
            previous.DeviceId != current.DeviceId)
        {
            throw new ArgumentException(
                "이전 상태와 현재 상태의 장비 ID가 다릅니다.",
                nameof(previous));
        }

        var alarms = new List<EquipmentAlarm>();

        AddAlarmWhenSeverityIncreases(
            alarms,
            current,
            AlarmType.HighTemperature,
            previous?.Temperature,
            current.Temperature,
            _thresholds.TemperatureWarning,
            _thresholds.TemperatureFault);

        AddAlarmWhenSeverityIncreases(
            alarms,
            current,
            AlarmType.HighVibration,
            previous?.Vibration,
            current.Vibration,
            _thresholds.VibrationWarning,
            _thresholds.VibrationFault);

        return alarms;
    }

    private static void AddAlarmWhenSeverityIncreases(
        ICollection<EquipmentAlarm> alarms,
        EquipmentSnapshot currentSnapshot,
        AlarmType alarmType,
        double? previousValue,
        double currentValue,
        double warningThreshold,
        double faultThreshold)
    {
        AlarmSeverity? previousSeverity =
            previousValue.HasValue
                ? DetermineSeverity(
                    previousValue.Value,
                    warningThreshold,
                    faultThreshold)
                : null;

        AlarmSeverity? currentSeverity =
            DetermineSeverity(
                currentValue,
                warningThreshold,
                faultThreshold);

        if (GetRank(currentSeverity) <=
            GetRank(previousSeverity))
        {
            return;
        }

        if (currentSeverity is null)
        {
            return;
        }

        double appliedThreshold =
            currentSeverity == AlarmSeverity.Fault
                ? faultThreshold
                : warningThreshold;

        alarms.Add(
            new EquipmentAlarm(
                currentSnapshot.DeviceId,
                currentSnapshot.Timestamp,
                alarmType,
                currentSeverity.Value,
                currentValue,
                appliedThreshold));
    }

    private static AlarmSeverity? DetermineSeverity(
        double value,
        double warningThreshold,
        double faultThreshold)
    {
        if (value >= faultThreshold)
        {
            return AlarmSeverity.Fault;
        }

        if (value >= warningThreshold)
        {
            return AlarmSeverity.Warning;
        }

        return null;
    }

    private static int GetRank(AlarmSeverity? severity)
    {
        return severity switch
        {
            null => 0,
            AlarmSeverity.Warning => 1,
            AlarmSeverity.Fault => 2,
            _ => throw new ArgumentOutOfRangeException(
                nameof(severity),
                severity,
                "지원하지 않는 알람 심각도입니다.")
        };
    }
}
