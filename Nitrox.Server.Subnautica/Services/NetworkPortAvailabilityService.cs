using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NitroxModel.Helper;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Service which waits for the configured port to be available.
/// </summary>
internal sealed class NetworkPortAvailabilityService(IOptions<Models.Configuration.SubnauticaServerOptions> options, ILogger<NetworkPortAvailabilityService> logger) : IHostedService
{
    private readonly Models.Configuration.SubnauticaServerOptions options = options.Value;
    private readonly ILogger<NetworkPortAvailabilityService> logger = logger;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await WaitForAvailablePortAsync(options.ServerPort, ct: cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task WaitForAvailablePortAsync(int port, TimeSpan timeout = default, CancellationToken ct = default)
    {
        if (timeout == default)
        {
            timeout = TimeSpan.FromSeconds(30);
        }
        else
        {
            Validate.IsTrue(timeout.TotalSeconds >= 5, "Timeout must be at least 5 seconds.");
        }

        DateTimeOffset time = DateTimeOffset.UtcNow;
        bool first = true;
        try
        {
            while (true)
            {
                ct.ThrowIfCancellationRequested();
                IPEndPoint endPoint = null;
                foreach (IPEndPoint ip in IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners())
                {
                    if (ip.Port == port)
                    {
                        endPoint = ip;
                        break;
                    }
                }
                if (endPoint == null)
                {
                    logger.LogDebug("Port {Port} UDP is available", options.ServerPort);
                    break;
                }

                if (first)
                {
                    first = false;
                    PrintPortWarn(logger, port, timeout);
                }
                else
                {
                    PrintPortWarn(logger, port, timeout - (DateTimeOffset.UtcNow - time));
                }

                await Task.Delay(3000, ct);
            }
        }
        catch (OperationCanceledException)
        {
            // ignored
        }

        static void PrintPortWarn(ILogger logger, int port, TimeSpan timeRemaining)
        {
            string message = $"Port {port} UDP is already in use. Please change the server port or close out any program that may be using it. Retrying for {Math.Floor(timeRemaining.TotalSeconds)} seconds until it is available...";
            logger.LogWarning(message);
        }
    }
}
