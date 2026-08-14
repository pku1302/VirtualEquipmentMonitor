using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace VirtualEquipmentMonitor.Infrastructure.Persistence;

public sealed class EquipmentDbContextFactory
    : IDesignTimeDbContextFactory<EquipmentDbContext>
{
    public EquipmentDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder =
            new DbContextOptionsBuilder<EquipmentDbContext>();

        optionsBuilder.UseSqlite(
            "Data Source=equipment-monitor.db");

        return new EquipmentDbContext(
            optionsBuilder.Options);
    }
}