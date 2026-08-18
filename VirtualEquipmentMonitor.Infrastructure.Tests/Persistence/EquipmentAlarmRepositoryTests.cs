
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using VirtualEquipmentMonitor.Domain.Alarms;
using VirtualEquipmentMonitor.Infrastructure.Persistence;

namespace VirtualEquipmentMonitor.Infrastructure.Tests.Persistence;

public sealed class EquipmentAlarmRepositoryTests
{
    [Fact]
    public async Task AddRangeAndGetRecentAsync_ReturnsNewestAlarms()
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

        var repository =
            new EquipmentAlarmRepository(
                new TestDbContextFactory(options));

        DateTimeOffset baseTime =
            new(
                2026,
                8,
                18,
                12,
                0,
                0,
                TimeSpan.Zero);

        EquipmentAlarm[] alarms =
        [
            CreateAlarm(
                baseTime,
                AlarmSeverity.Warning,
                76),

            CreateAlarm(
                baseTime.AddSeconds(1),
                AlarmSeverity.Fault,
                92)
        ];

        await repository.AddRangeAsync(
            alarms,
            CancellationToken.None);

        IReadOnlyList<EquipmentAlarm> result =
            await repository.GetRecentAsync(
                "EQ-001",
                count: 10,
                CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal(AlarmSeverity.Fault, result[0].Severity);
        Assert.Equal(AlarmSeverity.Warning, result[1].Severity);
    }

    private static EquipmentAlarm CreateAlarm(
        DateTimeOffset occuredAt,
        AlarmSeverity severity,
        double measuredValue)
    {
        double threshold =
            severity == AlarmSeverity.Fault
                ? 90
                : 75;

        return new EquipmentAlarm(
            "EQ-001",
            occuredAt,
            AlarmType.HighTemperature,
            severity,
            measuredValue,
            threshold);
    }
}
