using NitroxModel.Networking.Packets;

namespace NitroxClient.Communication.Packets.Processors.Core;

public interface IClientPacketProcessor<in TPacket> : IPacketProcessor<IPacketProcessContext, TPacket> where TPacket : Packet;
