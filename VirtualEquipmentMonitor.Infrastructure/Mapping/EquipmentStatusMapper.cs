using VirtualEquipmentMonitor.Contracts.Messages;
using VirtualEquipmentMonitor.Domain.Equipment;

namespace VirtualEquipmentMonitor.Infrastructure.Mapping;
public static class EquipmentStatusMapper
{
    public static EquipmentSnapshot ToDomain(
        EquipmentStatusMessage message)
    {
        return new EquipmentSnapshot(
            message.DeviceId,
            message.Timestamp,
            MapState(message.State),
            message.Temperature,
            message.Rpm,
            message.Vibration);
    }

    private static EquipmentState MapState(
        EquipmentOperatingState state)
    {
        return state switch
        {
            EquipmentOperatingState.Stopped =>
                EquipmentState.Stopped,

            EquipmentOperatingState.Running =>
                EquipmentState.Running,

            EquipmentOperatingState.Warning =>
                EquipmentState.Warning,

            EquipmentOperatingState.Fault =>
                EquipmentState.Fault,

            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "지원하지 않는 장비 상태입니다.")
        };
    }
}