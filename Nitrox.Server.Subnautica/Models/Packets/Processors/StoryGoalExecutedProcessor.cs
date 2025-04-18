using Microsoft.Extensions.Logging;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class StoryGoalExecutedProcessor(PlayerService playerService, StoryScheduleService storyScheduleService, ILogger<StoryGoalExecutedProcessor> logger)
    : AuthenticatedPacketProcessor<StoryGoalExecuted>
{
    private readonly PlayerService playerService = playerService;
    // TODO: USE DATABASE
    // private readonly StoryGoalData storyGoalData = storyGoalData;
    // private readonly PdaStateData pdaStateData = pdaStateData;
    private readonly StoryScheduleService storyScheduleService = storyScheduleService;
    private readonly ILogger<StoryGoalExecutedProcessor> logger = logger;

    public override void Process(StoryGoalExecuted packet, NitroxServer.Player player)
    {
        logger.LogDebug("Processing {Packet}", packet);
        // TODO: USE DATABASE
        // The switch is structure is similar to StoryGoal.Execute()
        // bool added = storyGoalData.CompletedGoals.Add(packet.Key);
        // switch (packet.Type)
        // {
        //     case StoryGoalExecuted.EventType.RADIO:
        //         if (added)
        //         {
        //             storyGoalData.RadioQueue.Enqueue(packet.Key);
        //         }
        //         break;
        //     case StoryGoalExecuted.EventType.PDA:
        //         if (packet.Timestamp.HasValue)
        //         {
        //             pdaStateData.AddPdaLogEntry(new(packet.Key, packet.Timestamp.Value));
        //         }
        //         break;
        // }

        storyScheduleService.UnScheduleGoal(packet.Key);

        playerService.SendPacketToOtherPlayers(packet, player);
    }
}
