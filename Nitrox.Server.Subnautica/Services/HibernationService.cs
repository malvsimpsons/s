using System.Threading;
using Microsoft.Extensions.Hosting;
using Nitrox.Server.Subnautica.Database.Models;
using Nitrox.Server.Subnautica.Models.Events;
using Nitrox.Server.Subnautica.Models.Events.Core;
using Nitrox.Server.Subnautica.Models.Respositories;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Service which can signal hibernating services to <see cref="Resume" /> or <see cref="Hibernate" />.
/// </summary>
internal sealed class HibernationService(ITrigger<ISeeHibernate, object> hibernators, ITrigger<ISeeResume, object> resumers, SessionRepository sessionRepository, ILogger<HibernationService> logger) : IHostedLifecycleService, ISeeSessionCreated, ISeeSessionDisconnected
{
    private readonly ITrigger<ISeeHibernate, object> hibernators = hibernators;
    private readonly ILogger<HibernationService> logger = logger;
    private readonly ITrigger<ISeeResume, object> resumers = resumers;
    private readonly SessionRepository sessionRepository = sessionRepository;
    private bool isHibernating;
    public int EventPriority => -1000;

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task Hibernate()
    {
        if (Interlocked.CompareExchange(ref isHibernating, true, false))
        {
            return;
        }
        logger.ZLogDebug($"Preparing to hibernate");
        await hibernators.Trigger();
        logger.ZLogDebug($"Now hibernating");
    }

    public async Task Resume()
    {
        if (!Interlocked.CompareExchange(ref isHibernating, false, true))
        {
            return;
        }
        logger.ZLogDebug($"Waking up");
        await resumers.Trigger();
        logger.ZLogDebug($"No longer hibernating");
    }

    public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task StartedAsync(CancellationToken cancellationToken) => await Hibernate();

    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async ValueTask HandleSessionDisconnect(Session disconnectedSession)
    {
        if (await sessionRepository.GetActiveSessionCount() > 0)
        {
            return;
        }
        _ = Task.Delay(250).ContinueWith(async _ => await Hibernate()).ContinueWithHandleError(ex => logger.ZLogError(ex, $"Error while trying to hibernate"));
    }

    public async ValueTask HandleSessionCreated(Session createdSession) => await Resume();
}
