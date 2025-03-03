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
///     Service which starts listening on the game server port. Passing incoming data to the packet handler.
/// </summary>
internal sealed class SubnauticaServerService(IOptions<ServerStartOptions> startOptions, PlayerService playerService, ILogger<SubnauticaServerService> logger) : IHostedLifecycleService
{
    private readonly ILogger<SubnauticaServerService> logger = logger;
    private readonly IOptions<ServerStartOptions> startOptions = startOptions;
    private readonly PlayerService playerService = playerService;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // TODO: Persist options

        // if (!stoppingToken.IsCancellationRequested)
        // {
        //     if (!server.Start(serverStartConfig.SaveName, CancellationTokenSource.CreateLinkedTokenSource(stoppingToken)))
        //     {
        //         throw new Exception("Unable to start server.");
        //     }
        //     else
        //     {
        //         // Log.Info($"Server started ({Math.Round(watch.Elapsed.TotalSeconds, 1)}s)");
        //
        //     }
        // }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        // TODO: Save & backup
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
