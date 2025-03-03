using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxServer.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

public class ScheduleProcessor(PlayerManager playerManager, ScheduleKeeper scheduleKeeper) : AuthenticatedPacketProcessor<Schedule>
{
    private readonly PlayerManager playerManager = playerManager;
    private readonly ScheduleKeeper scheduleKeeper = scheduleKeeper;

    public override void Process(Schedule packet, NitroxServer.Player player)
    {
        scheduleKeeper.ScheduleGoal(NitroxScheduledGoal.From(packet.TimeExecute, packet.Key, packet.Type));
        playerManager.SendPacketToOtherPlayers(packet, player);
    }
}