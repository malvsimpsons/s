using System;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Nitrox.Server.Subnautica.Models.Hibernation;
using Nitrox.Server.Subnautica.Models.Persistence;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Auto save service which persists the database.
/// </summary>
internal sealed class AutoSaveService(IPersistState state, ILogger<AutoSaveService> logger) : BackgroundService, IHibernate
{
    private readonly IPersistState state = state;
    private readonly ILogger<AutoSaveService> logger = logger;
    private readonly PeriodicTimer saveTimer = new(TimeSpan.FromMinutes(5)); // TODO: Use options to set save period.
    private bool isHibernating;

    public async Task Hibernate()
    {
        Interlocked.Exchange(ref isHibernating, true);
        await state.PersistState();
    }

    public Task Resume()
    {
        Interlocked.Exchange(ref isHibernating, false);
        return Task.CompletedTask;
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
}
