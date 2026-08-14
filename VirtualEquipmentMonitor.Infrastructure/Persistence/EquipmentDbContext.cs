using Microsoft.EntityFrameworkCore;
using VirtualEquipmentMonitor.Application.Abstractions.Persistence.Entities;

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
    }
}