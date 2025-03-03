using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Nitrox.Server.Subnautica.Services;

internal sealed class ServerPerformanceDiagnosticService([FromKeyedServices(typeof(ServerPerformanceDiagnosticService))] Stopwatch appStartStopWatch, ILogger<ServerPerformanceDiagnosticService> logger) : IHostedLifecycleService
{
    private readonly Stopwatch appStartStopWatch = appStartStopWatch;
    private readonly ILogger<ServerPerformanceDiagnosticService> logger = logger;

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartedAsync(CancellationToken cancellationToken)
    {
        appStartStopWatch.Stop();
        logger.LogInformation("Server started in {TimeSpan} seconds", Math.Round(appStartStopWatch.Elapsed.TotalSeconds, 3));
        return Task.CompletedTask;
    }

    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
