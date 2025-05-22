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
internal sealed class HibernationService(ITrigger<ISeeHibernate, object> hibernators, ITrigger<ISeeResume, object> resumers, SessionRepository sessionRepository) : IHostedLifecycleService, ISeeSessionCreated, ISeeSessionDisconnected
{
    private readonly ITrigger<ISeeHibernate, object> hibernators = hibernators;
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
        await hibernators.Trigger();
    }

    public async Task Resume()
    {
        if (!Interlocked.CompareExchange(ref isHibernating, false, true))
        {
            return;
        }
        await resumers.Trigger();
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
        await Hibernate();
    }

    public async ValueTask HandleSessionCreated(Session createdSession) => await Resume();
}
