using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace VirtualEquipmentMonitor.Infrastructure.Persistence;

public sealed class SqliteEquipmentDbContextFactory
    : IDbContextFactory<EquipmentDbContext>
{
    private readonly DbContextOptions<EquipmentDbContext> _options;

    public SqliteEquipmentDbContextFactory(
        string databasePath)
    {
        if (string.IsNullOrWhiteSpace(databasePath))
        {
            throw new ArgumentException(
                "DB 경로는 비어 있을 수 없습니다.",
                nameof(databasePath));
        }

        var connectionString =
            new SqliteConnectionStringBuilder
            {
                DataSource = databasePath
            }.ToString();

        _options =
            new DbContextOptionsBuilder<EquipmentDbContext>()
                .UseSqlite(connectionString)
                .Options;
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
