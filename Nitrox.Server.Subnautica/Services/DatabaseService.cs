using System;
using System.Data.Common;
using System.Threading;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nitrox.Server.Subnautica.Database;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Initializes the database and provides access to it by other services.
/// </summary>
internal sealed class DatabaseService(IHostEnvironment hostEnvironment, IDbContextFactory<WorldDbContext> dbContextFactory, ILogger<DatabaseService> logger) : IHostedLifecycleService
{
    private readonly IDbContextFactory<WorldDbContext> dbContextFactory = dbContextFactory;
    private readonly IHostEnvironment hostEnvironment = hostEnvironment;
    private readonly ILogger<DatabaseService> logger = logger;

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task StartingAsync(CancellationToken cancellationToken)
    {
        // Ensure database is up-to-date.
        await using WorldDbContext db = await GetDbContextAsync();
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

        // See https://sqlite.org/pragma.html
        await ExecutePragma(db, "synchronous=OFF");
        if (!hostEnvironment.IsDevelopment())
        {
            // In development mode, don't take exclusive lock. We might want to inspect the database while it's running - not just through this server but also via IDE.
            await ExecutePragma(db, "locking_mode=EXCLUSIVE");
        }
        await ExecutePragma(db, "temp_store=MEMORY");
        await ExecutePragma(db, "cache_size=-32000"); // Keep 32MB cache before writing to file.
        await ExecutePragma(db, "page_size=-32768");
    }

    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task<WorldDbContext> GetDbContextAsync() => await dbContextFactory.CreateDbContextAsync();

    private async Task ExecutePragma(WorldDbContext db, string pragmaCommand) => await ExecuteCommand(db, $"PRAGMA {pragmaCommand};");

    private async Task ExecuteCommand(WorldDbContext db, string command)
    {
        try
        {
            await using DbConnection connection = db.Database.GetDbConnection();
            await connection.OpenAsync();
            await using DbCommand commandObj = connection.CreateCommand();
            commandObj.CommandText = command;
            await commandObj.ExecuteNonQueryAsync();
            await connection.CloseAsync();
        }
        catch (Exception ex)
        {
            logger.LogWarning("Unable to execute command {Command}: {Error}", command, ex.Message);
        }
    }
}
