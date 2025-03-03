using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using NitroxModel.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

public class SignalPingPreferenceChangedProcessor : AuthenticatedPacketProcessor<SignalPingPreferenceChanged>
{
    public override void Process(SignalPingPreferenceChanged packet, NitroxServer.Player player)
    {
        player.PingInstancePreferences[packet.PingKey] = new(packet.Color, packet.Visible);
    }
}
