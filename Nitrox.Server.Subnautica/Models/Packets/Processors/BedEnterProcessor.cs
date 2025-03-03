using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using NitroxModel.Packets;
using NitroxServer.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

public class BedEnterProcessor(StoryManager storyManager) : AuthenticatedPacketProcessor<BedEnter>
{
    private readonly StoryManager storyManager = storyManager;

    public override void Process(BedEnter packet, NitroxServer.Player player)
    {
        // TODO: Needs repair since the new time implementation only relies on server-side time.
        // storyManager.ChangeTime(StoryManager.TimeModification.SKIP);
    }
}