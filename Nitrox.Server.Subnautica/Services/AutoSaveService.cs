using System;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Nitrox.Server.Subnautica.Models.Events;
using Nitrox.Server.Subnautica.Models.Persistence;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Auto save service which persists the database.
/// </summary>
internal sealed class AutoSaveService(IPersistState state, ILogger<AutoSaveService> logger) : BackgroundService, ISeeHibernate, ISeeResume, IHostedLifecycleService
{
    private readonly ILogger<AutoSaveService> logger = logger;
    private readonly PeriodicTimer saveTimer = new(TimeSpan.FromMinutes(5)); // TODO: Use options to set save period.
    private readonly IPersistState state = state;
    private bool isHibernating;
    private bool fullyStarted;

    public async ValueTask Hibernate()
    {
        Interlocked.Exchange(ref isHibernating, true);
        if (!fullyStarted)
        {
            return;
        }
        await Task.Delay(1000);
        await state.PersistState();
    }

    public ValueTask Resume()
    {
        Interlocked.Exchange(ref isHibernating, false);
        return ValueTask.CompletedTask;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.ZLogDebug($"Server will save over {saveTimer.Period}");
            await saveTimer.WaitForNextTickAsync(stoppingToken);
            if (Interlocked.CompareExchange(ref isHibernating, true, true))
            {
                continue;
            }
            logger.ZLogDebug($"Initiating save...");
            try
            {
                await state.PersistState();
            }
            catch (Exception ex)
            {
                logger.ZLogWarning(ex, $"Failed to save");
            }
        }
    }

    public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartedAsync(CancellationToken cancellationToken)
    {
        fullyStarted = true;
        return Task.CompletedTask;
    }

    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
