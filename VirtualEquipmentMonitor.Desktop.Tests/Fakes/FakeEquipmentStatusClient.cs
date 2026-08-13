using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using VirtualEquipmentMonitor.Application.Abstractions;
using VirtualEquipmentMonitor.Domain.Equipment;

namespace VirtualEquipmentMonitor.Desktop.Tests.Fakes;

public sealed class FakeEquipmentStatusClient
    : IEquipmentStatusClient
{
    private readonly IReadOnlyList<EquipmentSnapshot> _snapshots;
    private readonly bool _keepConnectionOpen;
    private readonly Exception? _exception;

    public FakeEquipmentStatusClient(
        IReadOnlyList<EquipmentSnapshot>? snapshots = null,
        bool keepConnectionOpen = false,
        Exception? exception = null)
    {
        _snapshots = snapshots ?? [];
        _keepConnectionOpen = keepConnectionOpen;
        _exception = exception;
    }

    public async IAsyncEnumerable<EquipmentSnapshot> ReceiveAsync(
        string host,
        int port,
        [EnumeratorCancellation]
        CancellationToken cancellationToken)
    {
        await Task.Yield();

        if (_exception is not null)
        {
            throw _exception;
        }

        foreach (var snapshot in _snapshots)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return snapshot;
        }

        if (_keepConnectionOpen)
        {
            await Task.Delay(
                Timeout.InfiniteTimeSpan,
                cancellationToken);
        }
    }
}