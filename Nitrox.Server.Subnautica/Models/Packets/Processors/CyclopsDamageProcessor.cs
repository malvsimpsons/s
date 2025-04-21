using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel_Subnautica.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

/// <summary>
///     This is the absolute damage state. The current simulation owner is the only one who sends this packet to the server
/// </summary>
internal sealed class CyclopsDamageProcessor : IAuthPacketProcessor<CyclopsDamage>
{
    public Task Process(AuthProcessorContext context, CyclopsDamage packet)
    {
        // TODO: Fix log
        Log.Debug($"New cyclops damage from {context.Sender} {packet}");

        context.ReplyToOthers(packet);
        return Task.CompletedTask;
    }
}
