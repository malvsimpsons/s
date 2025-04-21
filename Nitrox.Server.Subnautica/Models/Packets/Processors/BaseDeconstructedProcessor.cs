using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Networking.Packets;
using NitroxModel.Networking.Packets.Processors.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class BaseDeconstructedProcessor(GameLogic.Bases.BuildingManager buildingManager, PlayerService playerService) : IAuthPacketProcessor<BaseDeconstructed>
{
    private readonly GameLogic.Bases.BuildingManager buildingManager = buildingManager;
    private readonly PlayerService playerService = playerService;

    public Task Process(AuthProcessorContext context, BaseDeconstructed packet)
    {
        if (buildingManager.ReplaceBaseByGhost(packet))
        {
            playerService.SendPacketToOtherPlayers(packet, context.Sender);
        }
        return Task.CompletedTask;
    }
}
