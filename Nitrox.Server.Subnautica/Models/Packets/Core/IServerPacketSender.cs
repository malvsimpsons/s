using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Core;

public interface IServerPacketSender
{
    /// <summary>
    ///     Sends a packet to the given peer id, if connected.
    /// </summary>
    ValueTask SendPacket<T>(T packet, PeerId peerId) where T : Packet;

    /// <summary>
    ///     Sends a packet to the given session id.
    /// </summary>
    ValueTask SendPacket<T>(T packet, SessionId sessionId) where T : Packet;
    ValueTask SendPacketToAll<T>(T packet) where T : Packet;
    ValueTask SendPacketToOthers<T>(T packet, SessionId excludedSessionId) where T : Packet;
    ValueTask SendPacketToOthers<T>(T packet, PeerId excludedPeerId) where T : Packet;
}
