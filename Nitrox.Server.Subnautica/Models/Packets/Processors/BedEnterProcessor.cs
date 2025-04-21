using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Networking;
using NitroxModel.Networking.Packets;
using NitroxModel.Networking.Packets.Processors.Core;
using NitroxServer.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class BedEnterProcessor : IAuthPacketProcessor<BedEnter>
{
    public async Task Process(AuthProcessorContext context, BedEnter packet)
    {
        // TODO: Needs repair since the new time implementation only relies on server-side time.
        // storyTimingService.ChangeTime(StoryTimingService.TimeModification.SKIP);
    }
}
