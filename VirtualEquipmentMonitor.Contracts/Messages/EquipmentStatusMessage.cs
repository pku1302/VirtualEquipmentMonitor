using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualEquipmentMonitor.Contracts.Messages
{
    public sealed record EquipmentStatusMessage(
        string DeviceId,
        DateTimeOffset Timestamp,
        EquipmentOperatingState State,
        double Temperature,
        int Rpm,
        double Vibration);
}
