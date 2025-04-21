using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel_Subnautica.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class CyclopsDamagePointRepairedProcessor : IAuthPacketProcessor<CyclopsDamagePointRepaired>
{
    public Task Process(AuthProcessorContext context, CyclopsDamagePointRepaired packet)
    {
        context.ReplyToOthers(packet);
        return Task.CompletedTask;
    }
}
