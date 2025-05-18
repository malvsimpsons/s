using System;
using System.IO;
using System.Threading;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Nitrox.Server.Subnautica.Database;
using Nitrox.Server.Subnautica.Models.Configuration;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Initializes the database and provides access to it.
/// </summary>
internal sealed class DatabaseService(IDbContextFactory<WorldDbContext> dbContextFactory, IOptions<ServerStartOptions> startOptionsProvider, IHostEnvironment hostEnvironment, ILogger<DatabaseService> logger)
    : IHostedLifecycleService
{
    private readonly IDbContextFactory<WorldDbContext> dbContextFactory = dbContextFactory;
    private readonly TaskCompletionSource dbInit = new();
    private readonly IHostEnvironment hostEnvironment = hostEnvironment;
    private readonly ILogger<DatabaseService> logger = logger;
    private readonly IOptions<ServerStartOptions> startOptionsProvider = startOptionsProvider;

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartingAsync(CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await InitDatabase(cancellationToken);
            }
            catch (Exception ex)
            {
                dbInit.TrySetException(ex);
                throw;
            }
            if (!dbInit.TrySetResult())
            {
                throw new Exception("Failed to init database");
            }
        }, cancellationToken).ContinueWithHandleError(exception => logger.ZLogCritical(exception, $"Database initialization error"));
        return Task.CompletedTask;
    }

    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task StoppedAsync(CancellationToken cancellationToken)
    {
        await SaveAs();
    }

    public async Task<WorldDbContext> GetDbContextAsync()
    {
        await dbInit.Task; // Ensures database is initialized before other code can access it via context.
        return await dbContextFactory.CreateDbContextAsync();
    }

    public async Task<bool> SaveAs(string fileName = "world.db")
    {
        logger.ZLogInformation($"Saving database...");
        ServerStartOptions options = startOptionsProvider.Value;
        string mainSaveFilePath = Path.Combine(options.GetServerSavePath(), fileName);
        try
        {
            await using WorldDbContext db = await GetDbContextAsync();
            if (db.Database.GetDbConnection() is not SqliteConnection sqlite)
            {
                return false;
            }

            // Save to server save location.
            sqlite.Open();
            await using (SqliteConnection destination = new($"DataSource={mainSaveFilePath};"))
            {
                sqlite.BackupDatabase(destination);
            }
            logger.ZLogInformation($"Saved database to {mainSaveFilePath.ToSensitive():@FilePath}");

            // Create a copy into backup folder.
            // TODO: Backup config file
            // TODO: Change this to use options (e.g. MaxBackups)
            string backupFileName = DateTimeOffset.Now.ToString("O").Replace("T", " ");
            backupFileName = backupFileName.ReplaceInvalidFileNameCharacters();
            if (Path.GetExtension(backupFileName).Length > 4)
            {
                backupFileName = backupFileName.Replace('.', '\'');
            }
            backupFileName = Path.ChangeExtension(backupFileName, ".db");
            Directory.CreateDirectory(options.GetServerSaveBackupsPath());
            File.Copy(mainSaveFilePath, Path.Combine(options.GetServerSaveBackupsPath(), backupFileName));
            return true;
        }
        catch (Exception ex)
        {
            logger.ZLogError(ex, $"Error while trying to save the database");
        }

        return false;
    }

    private async Task InitDatabase(CancellationToken cancellationToken = default)
    {
        // Ensure database is up-to-date.
        await using WorldDbContext db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        if (hostEnvironment.IsDevelopment())
        {
            try
            {
                await db.Database.EnsureDeletedAsync(cancellationToken);
                await db.Database.EnsureCreatedAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.ZLogError(ex, $"Something is blocking the SQLite database. Check that you do not have it open in your IDE or other database viewer.");
                throw;
            }
        }
        else
        {
            await db.Database.MigrateAsync(cancellationToken);
        }
    }
}
