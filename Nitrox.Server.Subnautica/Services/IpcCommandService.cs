using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using NitroxModel.Core;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Opens an interprocess channel so other processes on the machine can send server commands.
/// </summary>
internal sealed class IpcCommandService(CommandService commandService, PlayerService playerService, ILogger<IpcCommandService> logger) : BackgroundService
{
    private readonly CommandService commandService = commandService;
    private readonly NamedPipeServerStream server = new($"Nitrox Server {NitroxEnvironment.CurrentProcessId}", PipeDirection.In, 1);
    private readonly PlayerService playerService = playerService;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogDebug("Starting IPC host for command input");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                string command = await ReadStringAsync(stoppingToken);
                commandService.ExecuteCommand(command, new HostToServerCommandContext(playerService));
            }
            catch (OperationCanceledException)
            {
                // ignored
            }
        }
    }

    public async Task<string> ReadStringAsync(CancellationToken cancellationToken = default)
    {
        if (!await IsConnectedAsync(cancellationToken))
        {
            return "";
        }

        try
        {
            byte[] sizeBytes = new byte[4];
            await server.ReadExactlyAsync(sizeBytes, cancellationToken);
            byte[] stringBytes = new byte[BitConverter.ToUInt32(sizeBytes)];
            await server.ReadExactlyAsync(stringBytes, cancellationToken);

            return Encoding.UTF8.GetString(stringBytes);
        }
        catch (Exception)
        {
            return "";
        }
    }

    private async Task<bool> IsConnectedAsync(CancellationToken cancellationToken)
    {
        if (server.IsConnected)
        {
            return true;
        }
        try
        {
            await server.WaitForConnectionAsync(cancellationToken);
            return true;
        }
        catch (IOException)
        {
            try
            {
                server.Disconnect();
            }
            catch (Exception)
            {
                // ignored
            }
        }
        return false;
    }
}
