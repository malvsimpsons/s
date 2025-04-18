using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nitrox.Server.Subnautica.Models.Configuration;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Service which prints out information at appropriate time in the app life cycle.
/// </summary>
internal sealed class GameServerStatusService(IOptions<ServerStartOptions> startOptions, PlayerService playerService, ILogger<GameServerStatusService> logger) : IHostedLifecycleService
{
    private readonly ILogger<GameServerStatusService> logger = logger;
    private readonly IOptions<ServerStartOptions> startOptions = startOptions;
    private readonly PlayerService playerService = playerService;

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
        return Task.CompletedTask;
    }

    public Task StoppingAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        playerService.SendPacketToAllPlayers(new ChatMessage(ChatMessage.SERVER_ID, "[BROADCAST] Server is shutting down..."));
        return Task.CompletedTask;
    }

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
