using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualEquipmentMonitor.Infrastructure.Persistence;
public sealed class EquipmentDatabaseInitializer
{
    private readonly IDbContextFactory<EquipmentDbContext>
        _contextFactory;

    public EquipmentDatabaseInitializer(
        IDbContextFactory<EquipmentDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        await using EquipmentDbContext context =
            await _contextFactory.CreateDbContextAsync(
                cancellationToken);

        await context.Database.MigrateAsync(
            cancellationToken);
    }
}
