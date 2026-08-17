using System;
using System.Collections.Generic;
using System.Text;
using VirtualEquipmentMonitor.Domain.Alarms;
using VirtualEquipmentMonitor.Domain.Equipment;

namespace VirtualEquipmentMonitor.Domain.Tests;

public sealed class EquipmentAlarmEvaluatorTests
{
    private readonly EquipmentAlarmEvaluator _evaluator =
        new(EquipmentThresholds.Default);

    [Fact]
    public void Evaluate_NormalToWarning_CreatesWarningAlarm()
    {
        var previous = CreateSnapshot(
            temperature: 70,
            vibration: 1);

        var current = CreateSnapshot(
            temperature: 76,
            vibration: 1);

        IReadOnlyList<EquipmentAlarm> alarms =
            _evaluator.Evaluate(previous, current);

        EquipmentAlarm alarm = Assert.Single(alarms);

        Assert.Equal(
            AlarmType.HighTemperature,
            alarm.Type);

        Assert.Equal(
            AlarmSeverity.Warning,
            alarm.Severity);

        Assert.Equal(76, alarm.MeasuredValue);
        Assert.Equal(75, alarm.Threshold);
    }

    [Fact]
    public void Evaluate_WarningToFault_CreatesFaultAlarm()
    {
        var previous = CreateSnapshot(
            temperature: 80,
            vibration: 1);

        var current = CreateSnapshot(
            temperature: 92,
            vibration: 1);

        IReadOnlyList<EquipmentAlarm> alarms =
            _evaluator.Evaluate(previous, current);

        EquipmentAlarm alarm = Assert.Single(alarms);

        Assert.Equal(
            AlarmType.HighTemperature,
            alarm.Type);

        Assert.Equal(
            AlarmSeverity.Fault,
            alarm.Severity);

        Assert.Equal(90, alarm.Threshold);
    }

    [Fact]
    public void Evaluate_RemainWarning_DoesNotCreateRepeatedAlarm()
    {
        var previous = CreateSnapshot(
            temperature: 76,
            vibration: 1);

        var current = CreateSnapshot(
            temperature: 80,
            vibration: 1);

        IReadOnlyList<EquipmentAlarm> alarms =
            _evaluator.Evaluate(previous, current);

        Assert.Empty(alarms);
    }

    [Fact]
    public void Evaluate_FaultToWarning_DoesNotCreateAlarm()
    {
        var previous = CreateSnapshot(
            temperature: 95,
            vibration: 1);

        var current = CreateSnapshot(
            temperature: 80,
            vibration: 1);

        IReadOnlyList<EquipmentAlarm> alarms =
            _evaluator.Evaluate(previous, current);

        Assert.Empty(alarms);
    }

    [Fact]
    public void Evaluate_FirstSnapshotAlreadyFault_CreatesAlarms()
    {
        var current = CreateSnapshot(
            temperature: 95,
            vibration: 5);

        IReadOnlyList<EquipmentAlarm> alarms =
            _evaluator.Evaluate(
                previous: null,
                current);

        Assert.Equal(2, alarms.Count);

        Assert.Contains(
            alarms,
            alarm =>
                alarm.Type == AlarmType.HighTemperature &&
                alarm.Severity == AlarmSeverity.Fault);

        Assert.Contains(
            alarms,
            alarm =>
                alarm.Type == AlarmType.HighVibration &&
                alarm.Severity == AlarmSeverity.Fault);
    }

    [Fact]
    public void Evaluate_DifferentDeviceIds_ThrowsException()
    {
        var previous = CreateSnapshot(
            temperature: 70,
            vibration: 1,
            deviceId: "EQ-001");

        var current = CreateSnapshot(
            temperature: 80,
            vibration: 1,
            deviceId: "EQ-002");

        Assert.Throws<ArgumentException>(
            () => _evaluator.Evaluate(
                previous,
                current));
    }

    private static EquipmentSnapshot CreateSnapshot(
        double temperature,
        double vibration,
        string deviceId = "EQ-001")
    {
        return new EquipmentSnapshot(
            deviceId,
            DateTimeOffset.UtcNow,
            EquipmentState.Running,
            temperature,
            1500,
            vibration);
    }
}
