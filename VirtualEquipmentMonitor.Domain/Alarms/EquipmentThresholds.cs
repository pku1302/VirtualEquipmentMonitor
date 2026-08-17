using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualEquipmentMonitor.Domain.Alarms;
public sealed record EquipmentThresholds
{
    public double TemperatureWarning { get; }
    public double TemperatureFault { get; }
    public double VibrationWarning { get; }
    public double VibrationFault { get; }
    public static EquipmentThresholds Default { get; } =
        new (
            temperatureWarning: 75.0,
            temperatureFault: 90.0,
            vibrationWarning: 3.5,
            vibrationFault: 4.5);

    public EquipmentThresholds(
        double temperatureWarning,
        double temperatureFault,
        double vibrationWarning,
        double vibrationFault)
    {
        ValidateRange(
            temperatureWarning,
            temperatureFault,
            nameof(temperatureWarning),
            nameof(temperatureFault));

        ValidateRange(
            vibrationWarning,
            vibrationFault,
            nameof(vibrationWarning),
            nameof(vibrationFault));

        TemperatureWarning = temperatureWarning;
        TemperatureFault = temperatureFault;
        VibrationWarning = vibrationWarning;
        VibrationFault = vibrationFault;
    }

    private static void ValidateRange(
        double warning,
        double fault,
        string warningParameterName,
        string faultParameterName)
    {
        if (warning < 0)
        {
            throw new ArgumentOutOfRangeException(
                warningParameterName,
                "Warning 임계값은 0보다 작을 수 없습니다.");
        }

        if (fault <= warning)
        {
            throw new ArgumentException(
                "Fault 임계값은 Warning 임계값보다 커야 합니다.",
                faultParameterName);
        }
    }
}
