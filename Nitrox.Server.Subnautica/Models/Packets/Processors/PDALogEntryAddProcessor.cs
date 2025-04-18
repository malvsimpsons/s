using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PDALogEntryAddProcessor(PlayerService playerManager, StoryScheduleService storyScheduleService) : AuthenticatedPacketProcessor<PDALogEntryAdd>
{
    private readonly PlayerService playerManager = playerManager;
    // TODO: USE DATABASE
    // private readonly IStateManager<PdaStateData> pda = pda;
    private readonly StoryScheduleService storyScheduleService = storyScheduleService;

    public override void Process(PDALogEntryAdd packet, NitroxServer.Player player)
    {
        // TODO: USE DATABASE
        // pda.GetStateAsync().GetAwaiter().GetResult().AddPdaLogEntry(new PdaLogEntry(packet.Key, packet.Timestamp));
        if (storyScheduleService.ContainsScheduledGoal(packet.Key))
        {
            storyScheduleService.UnScheduleGoal(packet.Key);
        }
        playerManager.SendPacketToOtherPlayers(packet, player);
    }
}
