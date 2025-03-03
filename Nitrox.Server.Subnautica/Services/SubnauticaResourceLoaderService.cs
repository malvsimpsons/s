using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nitrox.Server.Subnautica.Models.Resources;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Pre-warms Subnautica resources from files on server startup.
/// </summary>
internal class SubnauticaResourceLoaderService(IEnumerable<IGameResource> resources, ILogger<SubnauticaResourceLoaderService> logger) : IHostedService
{
    private readonly ILogger<SubnauticaResourceLoaderService> logger = logger;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogDebug("Loading {ResourceCount} resources {TypeNames}...", resources.Count(), string.Join(", ", resources.Select(r => r.GetType().Name).OrderBy(n => n)));
        await Parallel.ForEachAsync(resources, cancellationToken, async (resource, token) =>
        {
            string resourceName = resource.GetType().Name;

            Stopwatch stopwatch = Stopwatch.StartNew();
            await resource.LoadAsync(token);
            logger.LogDebug("Resource {TypeName} loaded in {Seconds} seconds", resourceName, Math.Round(stopwatch.Elapsed.TotalSeconds, 3));
        });
        logger.LogDebug("All game resources are loaded");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
