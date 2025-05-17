using NitroxModel.Networking.Packets;
using NitroxModel.Networking.Packets.Processors.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors.Core;

internal interface IAnonPacketProcessor : IPacketProcessor;

/// <summary>
///     A server packet processor that accepts anonymous connections.
/// </summary>
/// <typeparam name="TPacket">The packet type this processor can handle.</typeparam>
internal interface IAnonPacketProcessor<in TPacket> : IAnonPacketProcessor, IPacketProcessor<AnonProcessorContext, TPacket> where TPacket : Packet;
