using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxServer.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class ScheduleProcessor(PlayerService playerService, StoryScheduleService storyScheduleService) : AuthenticatedPacketProcessor<Schedule>
{
    private readonly PlayerService playerService = playerService;
    private readonly StoryScheduleService storyScheduleService = storyScheduleService;

    public override void Process(Schedule packet, NitroxServer.Player player)
    {
        storyScheduleService.ScheduleGoal(NitroxScheduledGoal.From(packet.TimeExecute, packet.Key, packet.Type));
        playerService.SendPacketToOtherPlayers(packet, player);
    }
}
