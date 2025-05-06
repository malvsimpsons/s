using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel_Subnautica.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class CyclopsFireCreatedProcessor : IAuthPacketProcessor<CyclopsFireCreated>
{
    public async Task Process(AuthProcessorContext context, CyclopsFireCreated packet)
    {
        await context.ReplyToOthers(packet);
    }
}
