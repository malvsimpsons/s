using System;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Nitrox.Server.Subnautica.Database.Models;
using Nitrox.Server.Subnautica.Models.Hibernation;
using Nitrox.Server.Subnautica.Models.Respositories;
using Nitrox.Server.Subnautica.Models.Respositories.Core;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Service which can signal hibernating services to <see cref="Resume" /> or <see cref="Hibernate" />.
/// </summary>
internal sealed class HibernationService(Func<IHibernate[]> hibernatorsProvider, SessionRepository sessionRepository, ILogger<HibernationService> logger) : IHostedLifecycleService, ISessionCleaner
{
    private readonly ILogger<HibernationService> logger = logger;
    private readonly SessionRepository sessionRepository = sessionRepository;
    private IHibernate[] hibernators;
    private bool isHibernating;
    public int SessionCleanPriority => -1000;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        hibernators ??= hibernatorsProvider();
        foreach (IHibernate hibernator in hibernators)
        {
            logger.ZLogTrace($"Added hibernator {hibernator.GetType().Name:@TypeName}");
        }
        logger.ZLogDebug($"{hibernators.Length:@HibernateCount} hibernators found and registered");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task Hibernate()
    {
        if (Interlocked.CompareExchange(ref isHibernating, true, false))
        {
            return;
        }
        logger.ZLogDebug($"Preparing to hibernate");
        await Task.WhenAll(hibernators.Select(h => h.Hibernate()));
        logger.ZLogDebug($"Now hibernating");
    }

    public async Task Resume()
    {
        if (!Interlocked.CompareExchange(ref isHibernating, false, true))
        {
            return;
        }
        logger.ZLogDebug($"Waking up");
        await Task.WhenAll(hibernators.Select(h => h.Resume()));
        logger.ZLogDebug($"No longer hibernating");
    }

    public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task StartedAsync(CancellationToken cancellationToken) => await Hibernate();

    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task CleanSessionAsync(Session disconnectedSession)
    {
        if (await sessionRepository.GetActiveSessionCount() > 0)
        {
            return;
        }
        _ = Task.Delay(250).ContinueWith(async _ => await Hibernate()).ContinueWithHandleError(ex => logger.ZLogError(ex, $"Error while trying to hibernate"));
    }
}
