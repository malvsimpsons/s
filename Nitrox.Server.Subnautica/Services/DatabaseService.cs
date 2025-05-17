using System;
using System.Data.Common;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Nitrox.Server.Subnautica.Database;
using Nitrox.Server.Subnautica.Models.Configuration;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Initializes the database and provides access to it.
/// </summary>
internal sealed class DatabaseService(IDbContextFactory<WorldDbContext> dbContextFactory, IOptions<SqliteOptions> optionsProvider, IOptions<ServerStartOptions> startOptionsProvider, IHostEnvironment hostEnvironment, ILogger<DatabaseService> logger)
    : IHostedLifecycleService
{
    private readonly IDbContextFactory<WorldDbContext> dbContextFactory = dbContextFactory;
    private readonly TaskCompletionSource dbInit = new();
    private readonly IHostEnvironment hostEnvironment = hostEnvironment;
    private readonly ILogger<DatabaseService> logger = logger;
    private readonly IOptions<SqliteOptions> optionsProvider = optionsProvider;
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

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task<WorldDbContext> GetDbContextAsync()
    {
        await dbInit.Task; // Ensures database is initialized before other code can access it via context.
        return await dbContextFactory.CreateDbContextAsync();
    }

    public async Task<bool> BackupAsync(string fileName)
    {
        Stopwatch sw = Stopwatch.StartNew();
        try
        {
            fileName = fileName.ReplaceInvalidFileNameCharacters();
            if (Path.GetExtension(fileName).Length > 4)
            {
                fileName = fileName.Replace('.', '\'');
            }
            fileName = Path.ChangeExtension(fileName, ".db");

            Directory.CreateDirectory(startOptionsProvider.Value.GetServerSaveBackupsPath());

            await using WorldDbContext db = await GetDbContextAsync();
            if (db.Database.GetDbConnection() is SqliteConnection sqlite)
            {
                await sqlite.OpenAsync();
                SqliteConnectionStringBuilder connectionBuilder = new() { DataSource = Path.Combine(startOptionsProvider.Value.GetServerSaveBackupsPath(), fileName) };
                await using SqliteConnection destination = new(connectionBuilder.ToString());
                sqlite.BackupDatabase(destination);
                return true;
            }
        }
        catch (Exception ex)
        {
            logger.ZLogError(ex, $"Error while trying to backup the database");
        }
        finally
        {
            sw.Stop();
            logger.ZLogInformation($"Saved backup as \"{fileName}\" which took {Math.Round(sw.Elapsed.TotalMilliseconds, 3)}ms");
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

        await ExecuteOptionsAsPragma(db, optionsProvider.Value);
    }

    private async Task ExecuteCommand(WorldDbContext db, string command)
    {
        try
        {
            logger.ZLogDebug($"Executing database command \"{command:@Command}\"");
            await using DbConnection connection = db.Database.GetDbConnection();
            await connection.OpenAsync();
            await using DbCommand commandObj = connection.CreateCommand();
            commandObj.CommandText = command;
            await commandObj.ExecuteNonQueryAsync();
            await connection.CloseAsync();
        }
        catch (Exception ex)
        {
            logger.ZLogWarning(ex, $"Unable to execute command {command:@Command}");
        }
    }

    private async Task ExecuteOptionsAsPragma(WorldDbContext db, SqliteOptions options)
    {
        StringBuilder pragmaBuilder = new();
        foreach (PropertyInfo property in options.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            ConfigurationKeyNameAttribute configKeyAttr = property.GetCustomAttribute<ConfigurationKeyNameAttribute>();
            string pragmaKey = configKeyAttr?.Name;
            if (string.IsNullOrWhiteSpace(pragmaKey))
            {
                continue;
            }
            string pragmaValue = property.GetValue(options)?.ToString() ?? "";
            if (string.IsNullOrWhiteSpace(pragmaValue))
            {
                continue;
            }

            pragmaBuilder
                .Append("PRAGMA ")
                .Append(pragmaKey)
                .Append('=')
                .Append(pragmaValue)
                .Append(';');
        }
        if (pragmaBuilder.Length > 0)
        {
            await ExecuteCommand(db, pragmaBuilder.ToString());
        }
    }
}
