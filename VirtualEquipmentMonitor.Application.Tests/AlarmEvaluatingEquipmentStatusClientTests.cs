using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using VirtualEquipmentMonitor.Application.Abstractions;
using VirtualEquipmentMonitor.Application.Abstractions.Persistence;
using VirtualEquipmentMonitor.Application.Services;
using VirtualEquipmentMonitor.Domain.Alarms;
using VirtualEquipmentMonitor.Domain.Equipment;

namespace VirtualEquipmentMonitor.Application.Tests;
public sealed class AlarmEvaluatingEquipmentStatusClientTests
{
    [Fact]
    public async Task ReceiveAsync_WhenSeverityIncreases_SavesAlarmOnce()
    {
        EquipmentSnapshot[] snapshots =
        [
            CreateSnapshot(temperature: 70),
            CreateSnapshot(temperature: 76),
            CreateSnapshot(temperature: 80),
        ];

        var innerClient =
            new StubStatusClient(snapshots);

        var repository =
            new RecordingAlarmRepository();

        var client =
            new AlarmEvaluatingEquipmentStatusClient(
                innerClient,
                new EquipmentAlarmEvaluator(
                    EquipmentThresholds.Default),
                repository);

        var received = new List<EquipmentSnapshot>();

        await foreach (EquipmentSnapshot snapshot in
            client.ReceiveAsync(
                "127.0.0.1",
                5000,
                CancellationToken.None))
        {
            received.Add(snapshot);
        }

        Assert.Equal(3, received.Count);

        EquipmentAlarm alarm =
            Assert.Single(repository.SavedAlarms);

        Assert.Equal(
            AlarmType.HighTemperature,
            alarm.Type);

        Assert.Equal(
            AlarmSeverity.Warning,
            alarm.Severity);
    }

    private static EquipmentSnapshot CreateSnapshot(
        double temperature)
    {
        return new EquipmentSnapshot(
            "EQ-001",
            DateTimeOffset.UtcNow,
            EquipmentState.Running,
            temperature,
            1500,
            1);
    }

    private sealed class StubStatusClient
        : IEquipmentStatusClient
    {
        private readonly IReadOnlyList<EquipmentSnapshot>
            _snapshots;

        public StubStatusClient(
            IReadOnlyList<EquipmentSnapshot> snapshots)
        {
            _snapshots = snapshots;
        }

        public async IAsyncEnumerable<EquipmentSnapshot>
            ReceiveAsync(
                string host,
                int port,
                [EnumeratorCancellation]
                CancellationToken cancellationToken)
        {
            foreach (EquipmentSnapshot snapshot in _snapshots)
            {
                await Task.Yield();

                cancellationToken
                    .ThrowIfCancellationRequested();

                yield return snapshot;
            }
        }
    }

    private sealed class RecordingAlarmRepository
        : IEquipmentAlarmRepository
    {
        public List<EquipmentAlarm> SavedAlarms { get; } = [];

        public Task AddRangeAsync(
            IReadOnlyCollection<EquipmentAlarm> alarms,
            CancellationToken cancellationToken)
        {
            SavedAlarms.AddRange(alarms);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<EquipmentAlarm>> GetRecentAsync(
            string deviceId,
            int count,
            CancellationToken cancellationToken)
        {
            IReadOnlyList<EquipmentAlarm> result =
                SavedAlarms
                    .Where(alarm =>
                        alarm.DeviceId == deviceId)
                    .OrderByDescending(alarm =>
                        alarm.OccuredAt)
                    .Take(count)
                    .ToList();

            return Task.FromResult(result);
        }
    }
}
