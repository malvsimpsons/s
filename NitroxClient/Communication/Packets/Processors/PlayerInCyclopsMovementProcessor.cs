using NitroxClient.GameLogic;
using NitroxModel_Subnautica.DataStructures;
using NitroxModel.Networking.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class PlayerInCyclopsMovementProcessor : IClientPacketProcessor<PlayerInCyclopsMovement>
{
    private readonly PlayerManager remotePlayerManager;

    public PlayerInCyclopsMovementProcessor(PlayerManager remotePlayerManager)
    {
        this.remotePlayerManager = remotePlayerManager;
    }

    public Task Process(IPacketProcessContext context, PlayerInCyclopsMovement movement)
    {
        if (remotePlayerManager.TryFind(movement.PlayerId, out RemotePlayer remotePlayer) && remotePlayer.Pawn != null)
        {
            remotePlayer.UpdatePositionInCyclops(movement.LocalPosition.ToUnity(), movement.LocalRotation.ToUnity());
        }
        return Task.CompletedTask;
    }
}
