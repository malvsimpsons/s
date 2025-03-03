using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using NitroxModel.Packets;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Unlockables;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

public class RadioPlayPendingMessageProcessor(StoryGoalData storyGoalData, PlayerManager playerManager) : AuthenticatedPacketProcessor<RadioPlayPendingMessage>
{
    private readonly StoryGoalData storyGoalData = storyGoalData;
    private readonly PlayerManager playerManager = playerManager;

    public override void Process(RadioPlayPendingMessage packet, NitroxServer.Player player)
    {
        if (!storyGoalData.RemovedLatestRadioMessage())
        {
            Log.Warn($"Tried to remove the latest radio message but the radio queue is empty: {packet}");
        }
        playerManager.SendPacketToOtherPlayers(packet, player);
    }
}