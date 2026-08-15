using System.Runtime.CompilerServices;
using VirtualEquipmentMonitor.Application.Abstractions;
using VirtualEquipmentMonitor.Application.Abstractions.Persistence;
using VirtualEquipmentMonitor.Domain.Equipment;

namespace VirtualEquipmentMonitor.Application.Services;

public sealed class PersistingEquipmentStatusClient
    : IEquipmentStatusClient
{
    private readonly IEquipmentStatusClient _innerClient;
    private readonly IEquipmentSnapshotRepository _repository;

    public PersistingEquipmentStatusClient(
        IEquipmentStatusClient innerClient,
        IEquipmentSnapshotRepository repository)
    {
        _innerClient = innerClient;
        _repository = repository;
    }
    public async IAsyncEnumerable<EquipmentSnapshot> ReceiveAsync(
        string host,
        int port,
        [EnumeratorCancellation]
        CancellationToken cancellationToken)
    {
        await foreach (EquipmentSnapshot snapshot in
            _innerClient.ReceiveAsync(
                host,
                port,
                cancellationToken))
        {
            await _repository.AddAsync(
                snapshot,
                cancellationToken);

            yield return snapshot;
        }
    }
}
