using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel_Subnautica.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class CyclopsDamagePointRepairedProcessor : IAuthPacketProcessor<CyclopsDamagePointRepaired>
{
    public async Task Process(AuthProcessorContext context, CyclopsDamagePointRepaired packet)
    {
        await context.ReplyToOthers(packet);
    }
}
