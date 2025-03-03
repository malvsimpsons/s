using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using NitroxModel.Packets;
using NitroxModel.Serialization;
using NitroxServer.Communication;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

public class MultiplayerSessionPolicyRequestProcessor(SubnauticaServerConfig config) : UnauthenticatedPacketProcessor<MultiplayerSessionPolicyRequest>
{
    private readonly SubnauticaServerConfig config = config;

    // This will extend in the future when we look into different options for auth
    public override void Process(MultiplayerSessionPolicyRequest packet, INitroxConnection connection)
    {
        Log.Info("Providing session policies...");
        connection.SendPacket(new MultiplayerSessionPolicy(packet.CorrelationId, config.DisableConsole, config.MaxConnections, config.IsPasswordRequired()));
    }
}