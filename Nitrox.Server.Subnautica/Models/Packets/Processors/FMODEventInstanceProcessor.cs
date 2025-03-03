using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using NitroxModel.DataStructures.Unity;
using NitroxModel.GameLogic.FMOD;
using NitroxModel.Packets;
using NitroxServer.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

public class FMODEventInstanceProcessor(PlayerManager playerManager, FmodWhitelist fmodWhitelist) : AuthenticatedPacketProcessor<FMODEventInstancePacket>
{
    private readonly PlayerManager playerManager = playerManager;
    private readonly FmodWhitelist fmodWhitelist = fmodWhitelist;

    public override void Process(FMODEventInstancePacket packet, NitroxServer.Player sendingPlayer)
    {
        if (!fmodWhitelist.TryGetSoundData(packet.AssetPath, out SoundData soundData))
        {
            Log.Error($"[{nameof(FMODEventInstanceProcessor)}] Whitelist has no item for {packet.AssetPath}.");
            return;
        }

        foreach (NitroxServer.Player player in playerManager.GetConnectedPlayers())
        {
            float distance = NitroxVector3.Distance(player.Position, packet.Position);
            if (player != sendingPlayer &&
                (soundData.IsGlobal || player.SubRootId.Equals(sendingPlayer.SubRootId)) &&
                distance < soundData.Radius)
            {
                packet.Volume = SoundHelper.CalculateVolume(distance, soundData.Radius, packet.Volume);
                player.SendPacket(packet);
            }
        }
    }
}
