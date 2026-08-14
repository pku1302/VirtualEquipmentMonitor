using Microsoft.EntityFrameworkCore;
using VirtualEquipmentMonitor.Application.Abstractions.Persistence;
using VirtualEquipmentMonitor.Application.Abstractions.Persistence.Entities;
using VirtualEquipmentMonitor.Domain.Equipment;

namespace VirtualEquipmentMonitor.Infrastructure.Persistence;

public sealed class EquipmentSnapshotRepository
    : IEquipmentSnapshotRepository
{
    private readonly IDbContextFactory<EquipmentDbContext>
        _contextFactory;

    public EquipmentSnapshotRepository(
        IDbContextFactory<EquipmentDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task AddAsync(
        EquipmentSnapshot snapshot,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        await using EquipmentDbContext context =
            await _contextFactory.CreateDbContextAsync(
                cancellationToken);

        var entity = ToEntity(snapshot);

        context.EquipmentSnapshots.Add(entity);

        await context.SaveChangesAsync(cancellationToken);
    }
    public async Task<IReadOnlyList<EquipmentSnapshot>> GetRecentAsync(
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

        List<EquipmentSnapshotEntity> entities =
            await context.EquipmentSnapshots
                .AsNoTracking()
                .Where(snapshot =>
                    snapshot.DeviceId == deviceId)
                .OrderByDescending(snapshot =>
                    snapshot.TimestampUtc)
                .Take(count)
                .ToListAsync(cancellationToken);

        return entities
            .Select(ToDomain)
            .ToList();
    }

    private static EquipmentSnapshotEntity ToEntity(
        EquipmentSnapshot snapshot)
    {
        return new EquipmentSnapshotEntity
        {
            DeviceId = snapshot.DeviceId,
            TimestampUtc = snapshot.Timestamp.UtcDateTime,
            State = snapshot.State.ToString(),
            Temperature = snapshot.Temperature,
            Rpm = snapshot.Rpm,
            Vibration = snapshot.Vibration
        };
    }
    private static EquipmentSnapshot ToDomain(
        EquipmentSnapshotEntity entity)
    {
        if (!Enum.TryParse(
            entity.State,
            ignoreCase: false,
            out EquipmentState state))
        {
            throw new InvalidDataException(
                $"저장된 장비 상태 '{entity.State}'가 올바르지 않습니다.");
        }

        DateTime utcTimestamp =
            DateTime.SpecifyKind(
                entity.TimestampUtc,
                DateTimeKind.Utc);

        return new EquipmentSnapshot(
            entity.DeviceId,
            new DateTimeOffset(
                utcTimestamp,
                TimeSpan.Zero),
            state,
            entity.Temperature,
            entity.Rpm,
            entity.Vibration);
    }
}