using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.DataStructures.Unity;
using NitroxModel.GameLogic.FMOD;
using NitroxModel.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class FmodEventInstanceProcessor(PlayerService playerService, FmodService fmodService) : AuthenticatedPacketProcessor<FMODEventInstancePacket>
{
    private readonly PlayerService playerService = playerService;
    private readonly FmodService fmodService = fmodService;

    public override void Process(FMODEventInstancePacket packet, NitroxServer.Player sendingPlayer)
    {
        if (!fmodService.TryGetSoundData(packet.AssetPath, out SoundData soundData))
        {
            Log.Error($"[{nameof(FmodEventInstanceProcessor)}] Whitelist has no item for {packet.AssetPath}.");
            return;
        }

        foreach (NitroxServer.Player player in playerService.GetConnectedPlayers())
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
