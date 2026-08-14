using Microsoft.EntityFrameworkCore;
using VirtualEquipmentMonitor.Infrastructure.Persistence;

namespace VirtualEquipmentMonitor.Infrastructure.Tests.Persistence;
internal sealed class TestDbContextFactory
    : IDbContextFactory<EquipmentDbContext>
{
    private readonly DbContextOptions<EquipmentDbContext>
        _options;

    public TestDbContextFactory(
        DbContextOptions<EquipmentDbContext> options)
    {
        _options = options;
    }
    public EquipmentDbContext CreateDbContext()
    {
        return new EquipmentDbContext(_options);
    }

    public Task<EquipmentDbContext> CreateDbContextAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(CreateDbContext());
    }
}
