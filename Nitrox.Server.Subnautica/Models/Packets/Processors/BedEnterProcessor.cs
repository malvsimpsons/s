using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Packets;
using NitroxServer.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class BedEnterProcessor(TimeService timeService) : AuthenticatedPacketProcessor<BedEnter>
{
    private readonly TimeService storyTimingService = timeService;

    public override void Process(BedEnter packet, NitroxServer.Player player)
    {
        // TODO: Needs repair since the new time implementation only relies on server-side time.
        // storyTimingService.ChangeTime(StoryTimingService.TimeModification.SKIP);
    }
}
