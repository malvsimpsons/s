using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class RadioPlayPendingMessageProcessor(PlayerService playerService) : AuthenticatedPacketProcessor<RadioPlayPendingMessage>
{
    // TODO: USE DATABASE
    // private readonly StoryGoalData storyGoalData = storyGoalData;
    private readonly PlayerService playerService = playerService;

    public override void Process(RadioPlayPendingMessage packet, NitroxServer.Player player)
    {
        // TODO: USE DATABASE
        // if (!storyGoalData.RemovedLatestRadioMessage())
        // {
        //     Log.Warn($"Tried to remove the latest radio message but the radio queue is empty: {packet}");
        // }
        playerService.SendPacketToOtherPlayers(packet, player);
    }
}
