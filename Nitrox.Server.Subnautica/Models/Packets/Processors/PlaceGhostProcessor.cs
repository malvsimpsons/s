using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Networking.Packets;
using NitroxServer.GameLogic.Bases;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PlaceGhostProcessor(GameLogic.Bases.BuildingManager buildingManager) : IAuthPacketProcessor<PlaceGhost>
{
    public async Task Process(AuthProcessorContext context, PlaceGhost packet)
    {
        if (buildingManager.AddGhost(packet))
        {
            context.ReplyToOthers(packet);
        }
    }
}
