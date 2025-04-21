using System.Threading;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Nitrox.Server.Subnautica.Database;

namespace Nitrox.Server.Subnautica.Services;

internal sealed class DatabaseService(IDbContextFactory<WorldDbContext> dbContextFactory, IHostEnvironment hostEnvironment) : IHostedLifecycleService
{
    private readonly IDbContextFactory<WorldDbContext> dbContextFactory = dbContextFactory;
    private readonly IHostEnvironment hostEnvironment = hostEnvironment;
    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task StartingAsync(CancellationToken cancellationToken)
    {
        // Ensure database is up-to-date.
        WorldDbContext db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        if (hostEnvironment.IsDevelopment())
        {
            // In development, ensure database is up-to-date with latest EF model. Not preserving data is (usually) fine.
            await db.Database.EnsureDeletedAsync(cancellationToken);
            await db.Database.EnsureCreatedAsync(cancellationToken);
        }
        else
        {
            await db.Database.MigrateAsync(cancellationToken);
        }
    }

    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken)
    {
        SqliteConnection.ClearAllPools(); // Will tell sqlite to hurry up and finalize. Causes this app to close faster. See https://github.com/dotnet/efcore/issues/26580#issuecomment-2668483600
        return Task.CompletedTask;
    }
}
