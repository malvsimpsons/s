using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nitrox.Server.Subnautica.Models.Configuration;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using NitroxModel.Core;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Service which prints out information at appropriate time in the app life cycle.
/// </summary>
internal sealed class ServerStatusService([FromKeyedServices(typeof(ServerStatusService))] Stopwatch appStartStopWatch, IOptions<ServerStartOptions> startOptions, IServerPacketSender packetSender, ILogger<ServerStatusService> logger) : IHostedLifecycleService
{
    private readonly ILogger<ServerStatusService> logger = logger;
    private readonly IOptions<ServerStartOptions> startOptions = startOptions;
    private readonly IServerPacketSender packetSender = packetSender;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StartingAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting Nitrox server {ReleasePhase} v{Version} for {GameName}", NitroxEnvironment.ReleasePhase, NitroxEnvironment.Version, GameInfo.Subnautica.FullName);
        logger.LogInformation("Using game files from {Path}", startOptions.Value.GameInstallPath);
        logger.LogInformation("Using world name {SaveName}", startOptions.Value.SaveName);
        return Task.CompletedTask;
    }

    public Task StartedAsync(CancellationToken cancellationToken)
    {
        appStartStopWatch.Stop();
        logger.LogInformation("Server started in {TimeSpan} seconds", Math.Round(appStartStopWatch.Elapsed.TotalSeconds, 3));
        return Task.CompletedTask;
    }

    public Task StoppingAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        packetSender.SendPacketToAll(new ChatMessage(SessionId.SERVER_ID, "[BROADCAST] Server is shutting down..."));
        return Task.CompletedTask;
    }

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
