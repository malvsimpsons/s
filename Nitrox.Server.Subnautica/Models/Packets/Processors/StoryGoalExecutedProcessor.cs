using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Packets;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Unlockables;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class StoryGoalExecutedProcessor(PlayerService playerService, StoryGoalData storyGoalData, ScheduleKeeper scheduleKeeper, PDAStateData pdaStateData)
    : AuthenticatedPacketProcessor<StoryGoalExecuted>
{
    private readonly PlayerService playerService = playerService;
    private readonly StoryGoalData storyGoalData = storyGoalData;
    private readonly ScheduleKeeper scheduleKeeper = scheduleKeeper;
    private readonly PDAStateData pdaStateData = pdaStateData;

    public override void Process(StoryGoalExecuted packet, NitroxServer.Player player)
    {
        Log.Debug($"Processing StoryGoalExecuted: {packet}");
        // The switch is structure is similar to StoryGoal.Execute()
        bool added = storyGoalData.CompletedGoals.Add(packet.Key);
        switch (packet.Type)
        {
            case StoryGoalExecuted.EventType.RADIO:
                if (added)
                {
                    storyGoalData.RadioQueue.Enqueue(packet.Key);
                }
                break;
            case StoryGoalExecuted.EventType.PDA:
                if (packet.Timestamp.HasValue)
                {
                    pdaStateData.AddPdaLogEntry(new(packet.Key, packet.Timestamp.Value));
                }
                break;
        }

        scheduleKeeper.UnScheduleGoal(packet.Key);

        playerService.SendPacketToOtherPlayers(packet, player);
    }
}
