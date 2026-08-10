using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualEquipmentMonitor.Domain.Equipment
{
    public sealed record EquipmentSnapshot
    {
        public string DeviceId { get; }
        public DateTimeOffset Timestamp { get; }
        public EquipmentState State { get; }
        public double Temperature { get; }
        public int Rpm { get; }
        public double Vibration { get; }

        public EquipmentSnapshot(
            string deviceId,
            DateTimeOffset timestamp,
            EquipmentState state,
            double temperature,
            int rpm,
            double vibration)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
            {
                throw new ArgumentException(
                    "장비 ID는 비어 있을 수 없습니다.",
                    nameof(deviceId));
            }

            if (rpm < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(rpm),
                    "RPM은 0보다 작을 수 없습니다.");
            }

            if (vibration < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(vibration),
                    "진동값은 0보다 작을 수 없습니다.");
            }

            DeviceId = deviceId;
            Timestamp = timestamp;
            State = state;
            Temperature = temperature;
            Rpm = rpm;
            Vibration = vibration;
        }
    }
}
