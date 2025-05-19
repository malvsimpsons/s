using System;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Nitrox.Server.Subnautica.Models.Configuration;
using Nitrox.Server.Subnautica.Models.Events;
using Nitrox.Server.Subnautica.Models.Persistence;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Auto save service which persists the database.
/// </summary>
internal sealed class AutoSaveService(IPersistState state, IOptionsMonitor<SubnauticaServerOptions> optionsProvider, ILogger<AutoSaveService> logger) : BackgroundService, ISeeHibernate, ISeeResume, IHostedLifecycleService
{
    private readonly ILogger<AutoSaveService> logger = logger;
    private readonly PeriodicTimer saveTimer = new(TimeSpan.FromMinutes(5));
    private readonly IPersistState state = state;
    private CancellationTokenSource cts;
    private readonly IOptionsMonitor<SubnauticaServerOptions> optionsProvider = optionsProvider;
    private bool fullyStarted;
    private bool isHibernating;

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

    public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartedAsync(CancellationToken cancellationToken)
    {
        fullyStarted = true;
        return Task.CompletedTask;
    }

    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        IDisposable configDisposable = optionsProvider.OnChange(ConfigChanged);
        int initialPeriodMs = optionsProvider.CurrentValue.SaveInterval;
        if (initialPeriodMs < 1000)
        {
            initialPeriodMs = Timeout.Infinite;
        }
        saveTimer.Period = TimeSpan.FromMilliseconds(initialPeriodMs);

        try
        {
            cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (!Interlocked.CompareExchange(ref isHibernating, true, true))
                    {
                        if (saveTimer.Period != Timeout.InfiniteTimeSpan)
                        {
                            logger.ZLogDebug($"Server will save over {saveTimer.Period}");
                        }
                        await saveTimer.WaitForNextTickAsync(cts.Token);
                    }
                    else
                    {
                        await saveTimer.WaitForNextTickAsync(cts.Token);
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
                catch (OperationCanceledException)
                {
                    cts?.Dispose();
                    cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                }
            }
        }
        finally
        {
            cts?.Dispose();
            configDisposable?.Dispose();
        }
    }

    private void ConfigChanged(SubnauticaServerOptions options, string arg2)
    {
        int initialPeriodMs = options.SaveInterval;
        if (initialPeriodMs < 1000)
        {
            initialPeriodMs = Timeout.Infinite;
        }
        TimeSpan newPeriod = TimeSpan.FromMilliseconds(initialPeriodMs);
        if (newPeriod == Timeout.InfiniteTimeSpan && saveTimer.Period != Timeout.InfiniteTimeSpan)
        {
            logger.ZLogInformation($"disabled");
        }
        else if (saveTimer.Period == Timeout.InfiniteTimeSpan && newPeriod != Timeout.InfiniteTimeSpan)
        {
            logger.ZLogInformation($"enabled");
        }
        else if (newPeriod != Timeout.InfiniteTimeSpan)
        {
            logger.ZLogInformation($"Updated save period from {saveTimer.Period} to {newPeriod}");
        }
        saveTimer.Period = newPeriod;
        cts.Cancel();
    }
}
