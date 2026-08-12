using VirtualEquipmentMonitor.Domain.Equipment;

namespace VirtualEquipmentMonitor.Application.Abstractions;

public interface IEquipmentStatusClient
{
    IAsyncEnumerable<EquipmentSnapshot> ReceiveAsync(
        string host,
        int port,
        CancellationToken cancellationToken);
}
