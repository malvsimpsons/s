using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nitrox.Server.Subnautica.Models.Configuration;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using NitroxModel.Packets;
using NitroxServer.Communication;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class MultiplayerSessionPolicyRequestProcessor(IOptions<SubnauticaServerOptions> optionsProvider, ILogger<MultiplayerSessionPolicyRequestProcessor> logger) : UnauthenticatedPacketProcessor<MultiplayerSessionPolicyRequest>
{
    private readonly IOptions<SubnauticaServerOptions> optionsProvider = optionsProvider;
    private readonly ILogger<MultiplayerSessionPolicyRequestProcessor> logger = logger;

    // This will extend in the future when we look into different options for auth
    public override void Process(MultiplayerSessionPolicyRequest packet, INitroxConnection connection)
    {
        logger.LogInformation("Providing session policies...");
        SubnauticaServerOptions options = optionsProvider.Value;
        connection.SendPacket(new MultiplayerSessionPolicy(packet.CorrelationId, options.DisableConsole, options.MaxConnections, options.IsPasswordRequired()));
    }
}
