using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Nitrox.Server.Subnautica.Models.Hibernation;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Service which can signal hibernating services to <see cref="Resume" /> or <see cref="Hibernate" />.
/// </summary>
internal sealed class HibernationService(IEnumerable<IHibernate> hibernators, ILogger<HibernationService> logger) : IHostedService
{
    private readonly IHibernate[] hibernators = [..hibernators];
    private readonly ILogger<HibernationService> logger = logger;

    public Task StartAsync(CancellationToken cancellationToken)
    {
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
        foreach (IHibernate hibernator in hibernators)
        {
            await hibernator.Hibernate();
        }
    }

    public async Task Resume()
    {
        foreach (IHibernate hibernator in hibernators)
        {
            await hibernator.Resume();
        }
    }
}
