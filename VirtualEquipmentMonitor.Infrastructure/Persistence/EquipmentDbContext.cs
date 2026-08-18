using Microsoft.EntityFrameworkCore;
using VirtualEquipmentMonitor.Application.Abstractions.Persistence.Entities;
using VirtualEquipmentMonitor.Infrastructure.Persistence.Entities;

namespace VirtualEquipmentMonitor.Infrastructure.Persistence;

public sealed class EquipmentDbContext : DbContext
{
    public EquipmentDbContext(
        DbContextOptions<EquipmentDbContext> options)
        : base(options)
    {

    }

    public DbSet<EquipmentSnapshotEntity> EquipmentSnapshots =>
        Set<EquipmentSnapshotEntity>();

    public DbSet<EquipmentAlarmEntity> EquipmentAlarms =>
        Set<EquipmentAlarmEntity>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        var entity =
            modelBuilder.Entity<EquipmentSnapshotEntity>();

        entity.ToTable("EquipmentSnapshots");

        entity.HasKey(snapshot => snapshot.Id);

        entity.Property(snapshot => snapshot.DeviceId)
            .HasMaxLength(50)
            .IsRequired();

        entity.Property(snapshot => snapshot.TimestampUtc)
            .IsRequired();

        entity.Property(snapshot => snapshot.State)
            .HasMaxLength(20)
            .IsRequired();

        entity.Property(snapshot => snapshot.Rpm)
            .IsRequired();

        entity.HasIndex(snapshot => new
        {
            snapshot.DeviceId,
            snapshot.TimestampUtc
        });

        var alarm =
            modelBuilder.Entity<EquipmentAlarmEntity>();

        alarm.ToTable("EquipmentAlarms");

        alarm.HasKey(item => item.Id);
        
        alarm.Property(item => item.DeviceId)
            .HasMaxLength(50)
            .IsRequired();
        
        alarm.Property(item => item.OccurredAtUtc)
            .IsRequired();
        
        alarm.Property(item => item.Type)
            .HasMaxLength(50)
            .IsRequired();

        alarm.Property(item => item.Severity)
            .HasMaxLength(20)
            .IsRequired();

        alarm.Property(item => item.MeasuredValue)
            .IsRequired();

        alarm.Property(item => item.Threshold)
            .IsRequired();

        alarm.HasIndex(item => new
        {
            item.DeviceId,
            item.OccurredAtUtc
        });
    }
}