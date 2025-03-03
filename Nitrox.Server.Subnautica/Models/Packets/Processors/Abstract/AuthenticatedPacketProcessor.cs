using NitroxModel.Packets;
using NitroxModel.Packets.Processors.Abstract;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;

public abstract class AuthenticatedPacketProcessor<T> : PacketProcessor where T : Packet
{
    public override void ProcessPacket(Packet packet, IProcessorContext player)
    {
        Process((T)packet, (NitroxServer.Player)player);
    }

    public abstract void Process(T packet, NitroxServer.Player player);
}
