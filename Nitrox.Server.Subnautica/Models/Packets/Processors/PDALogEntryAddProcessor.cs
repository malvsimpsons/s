using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Models.Persistence.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Unlockables;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PDALogEntryAddProcessor(PlayerService playerManager, IStateManager<PDAStateData> pda, ScheduleKeeper scheduleKeeper) : AuthenticatedPacketProcessor<PDALogEntryAdd>
{
    private readonly PlayerService playerManager = playerManager;
    private readonly IStateManager<PDAStateData> pda = pda;
    private readonly ScheduleKeeper scheduleKeeper = scheduleKeeper;

    public override void Process(PDALogEntryAdd packet, NitroxServer.Player player)
    {
        pda.GetStateAsync().GetAwaiter().GetResult().AddPdaLogEntry(new PdaLogEntry(packet.Key, packet.Timestamp));
        if (scheduleKeeper.ContainsScheduledGoal(packet.Key))
        {
            scheduleKeeper.UnScheduleGoal(packet.Key);
        }
        playerManager.SendPacketToOtherPlayers(packet, player);
    }
}
