using Microsoft.EntityFrameworkCore;
using VirtualEquipmentMonitor.Application.Abstractions.Persistence;
using VirtualEquipmentMonitor.Domain.Alarms;
using VirtualEquipmentMonitor.Infrastructure.Persistence.Entities;

namespace VirtualEquipmentMonitor.Infrastructure.Persistence;
public sealed class EquipmentAlarmRepository
    : IEquipmentAlarmRepository
{
    private readonly IDbContextFactory<EquipmentDbContext>
        _contextFactory;

    public EquipmentAlarmRepository(
        IDbContextFactory<EquipmentDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task AddRangeAsync(
        IReadOnlyCollection<EquipmentAlarm> alarms,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(alarms);

        if (alarms.Count == 0)
        {
            return;
        }

        await using EquipmentDbContext context =
            await _contextFactory.CreateDbContextAsync(
                cancellationToken);

        IEnumerable<EquipmentAlarmEntity> entities =
            alarms.Select(ToEntity);

        context.EquipmentAlarms.AddRange(entities);

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EquipmentAlarm>> GetRecentAsync(
        string deviceId,
        int count,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            throw new ArgumentException(
                "장비 ID는 비어 있을 수 없습니다.",
                nameof(deviceId));
        }

        if (count <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(count),
                "조회 개수는 0보다 커야 합니다.");
        }

        await using EquipmentDbContext context =
            await _contextFactory.CreateDbContextAsync(
                cancellationToken);

        List<EquipmentAlarmEntity> entities =
            await context.EquipmentAlarms
                .AsNoTracking()
                .Where(alarm =>
                    alarm.DeviceId == deviceId)
                .OrderByDescending(alarm =>
                    alarm.OccurredAtUtc)
                .Take(count)
                .ToListAsync(cancellationToken);

        return entities
            .Select(ToDomain)
            .ToList();
    }

    private static EquipmentAlarmEntity ToEntity(
        EquipmentAlarm alarm)
    {
        return new EquipmentAlarmEntity
        {
            DeviceId = alarm.DeviceId,
            OccurredAtUtc = alarm.OccuredAt.UtcDateTime,
            Type = alarm.Type.ToString(),
            Severity = alarm.Severity.ToString(),
            MeasuredValue = alarm.MeasuredValue,
            Threshold = alarm.Threshold
        };
    }

    private static EquipmentAlarm ToDomain(
        EquipmentAlarmEntity entity)
    {
        if (!Enum.TryParse(
                entity.Type,
                out AlarmType type))
        {
            throw new InvalidDataException(
                $"알 수 없는 알람 종류입니다: {entity.Type}");
        }

        if (!Enum.TryParse(
                entity.Severity,
                out AlarmSeverity severity))
        {
            throw new InvalidDataException(
                $"알 수 없는 알람 심각도입니다: {entity.Severity}");
        }

        DateTime occurredAtUtc =
            DateTime.SpecifyKind(
                entity.OccurredAtUtc,
                DateTimeKind.Utc);

        return new EquipmentAlarm(
            entity.DeviceId,
            new DateTimeOffset(
                occurredAtUtc,
                TimeSpan.Zero),
            type,
            severity,
            entity.MeasuredValue,
            entity.Threshold);
    }
}
