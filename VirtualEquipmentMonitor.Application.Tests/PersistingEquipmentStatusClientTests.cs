
using System.Runtime.CompilerServices;
using VirtualEquipmentMonitor.Application.Abstractions;
using VirtualEquipmentMonitor.Application.Abstractions.Persistence;
using VirtualEquipmentMonitor.Application.Services;
using VirtualEquipmentMonitor.Domain.Equipment;

namespace VirtualEquipmentMonitor.Application.Tests;
public sealed class PersistingEquipmentStatusClientTests
{
    [Fact]
    public async Task ReceiveAsync_WhenSnapshotArrives_SavesAndReturnsIt()
    {
        var snapshot = new EquipmentSnapshot(
            "EQ-TEST",
            DateTimeOffset.UtcNow,
            EquipmentState.Running,
            42.5,
            1500,
            1.25);

        var innerClient =
            new StubEquipmentStatusClient(snapshot);

        var repository =
            new RecordingEquipmentSnapshotRepository();

        var client =
            new PersistingEquipmentStatusClient(
                innerClient,
                repository);

        var receivedSnapshots =
            new List<EquipmentSnapshot>();

        await foreach (EquipmentSnapshot received in
            client.ReceiveAsync(
                "127.0.0.1",
                5000,
                CancellationToken.None))
        {
            receivedSnapshots.Add(received);
        }

        EquipmentSnapshot receivedSnapshot =
            Assert.Single(receivedSnapshots);

        EquipmentSnapshot savedSnapshot =
            Assert.Single(repository.SavedSnapshots);

        Assert.Same(snapshot, receivedSnapshot);
        Assert.Same(snapshot, savedSnapshot);
    }

    private sealed class StubEquipmentStatusClient
        : IEquipmentStatusClient
    {
        private readonly EquipmentSnapshot _snapshot;

        public StubEquipmentStatusClient(
            EquipmentSnapshot snapshot)
        {
            _snapshot = snapshot;
        }

        public async IAsyncEnumerable<EquipmentSnapshot>
            ReceiveAsync(
                string host,
                int port,
                [EnumeratorCancellation]
                CancellationToken cancellationToken)
        {
            await Task.Yield();

            cancellationToken
                .ThrowIfCancellationRequested();

            yield return _snapshot;
        }
    }

    private sealed class RecordingEquipmentSnapshotRepository
        : IEquipmentSnapshotRepository
    {
        public List<EquipmentSnapshot> SavedSnapshots
        {
            get;
        } = [];

        public Task AddAsync(
            EquipmentSnapshot snapshot,
            CancellationToken cancellationToken)
        {
            cancellationToken
                .ThrowIfCancellationRequested();

            SavedSnapshots.Add(snapshot);

            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<EquipmentSnapshot>>
            GetRecentAsync(
                string deviceId,
                int count,
                CancellationToken cancellationToken)
        {
            IReadOnlyList<EquipmentSnapshot> result =
                SavedSnapshots
                    .Where(snapshot =>
                        snapshot.DeviceId == deviceId)
                    .TakeLast(count)
                    .Reverse()
                    .ToList();

            return Task.FromResult(result);
        }
    }
}
