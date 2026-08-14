using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using VirtualEquipmentMonitor.Domain.Equipment;
using VirtualEquipmentMonitor.Infrastructure.Persistence;

namespace VirtualEquipmentMonitor.Infrastructure.Tests.Persistence;

public sealed class EquipmentSnapshotRepositoryTests
{
    [Fact]
    public async Task AddAndGetRecentAsync_ReturnsNewestSnapshots()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        var options =
            new DbContextOptionsBuilder<EquipmentDbContext>()
                .UseSqlite(connection)
                .Options;

        await using (var context =
                    new EquipmentDbContext(options))
        {
            await context.Database.EnsureCreatedAsync();
        }

        var factory =
            new TestDbContextFactory(options);

        var repository =
            new EquipmentSnapshotRepository(factory);

        DateTimeOffset baseTime =
            new(
                2026,
                8,
                14,
                12,
                0,
                0,
                TimeSpan.Zero);

        await repository.AddAsync(
            CreateSnapshot(
                "EQ-001",
                baseTime,
                temperature: 30),
            CancellationToken.None);

        await repository.AddAsync(
            CreateSnapshot(
                "EQ-001",
                baseTime.AddSeconds(1),
                temperature: 40),
            CancellationToken.None);

        await repository.AddAsync(
            CreateSnapshot(
                "EQ-001",
                baseTime.AddSeconds(2),
                temperature: 50),
            CancellationToken.None);

        await repository.AddAsync(
            CreateSnapshot(
                "EQ-OTHER",
                baseTime.AddSeconds(3),
                temperature: 99),
            CancellationToken.None);

        IReadOnlyList<EquipmentSnapshot> result =
            await repository.GetRecentAsync(
                "EQ-001",
                count: 2,
                CancellationToken.None);

        Assert.Equal(2, result.Count);

        Assert.Equal(50, result[0].Temperature);
        Assert.Equal(40, result[1].Temperature);

        Assert.All(
            result,
            snapshot =>
                Assert.Equal("EQ-001", snapshot.DeviceId));

        Assert.Equal(
            baseTime.AddSeconds(2),
            result[0].Timestamp);
    }

    [Fact]
    public async Task GetRecentAsync_WithInvalidCount_ThrowsException()
    {
        var options =
            new DbContextOptionsBuilder<EquipmentDbContext>()
                .UseSqlite("Data Source=:memory")
                .Options;

        var factory =
            new TestDbContextFactory(options);

        var repository =
            new EquipmentSnapshotRepository(factory);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => repository.GetRecentAsync(
                "EQ-001",
                count: 0,
                CancellationToken.None));
    }

    private static EquipmentSnapshot CreateSnapshot(
        string deviceId,
        DateTimeOffset timestamp,
        double temperature)
    {
        return new EquipmentSnapshot(
            deviceId,
            timestamp,
            EquipmentState.Running,
            temperature,
            1500,
            1.25);
    }
}
