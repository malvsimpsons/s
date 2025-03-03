using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using NitroxModel.DataStructures.Unity;
using NitroxModel.GameLogic.FMOD;
using NitroxModel.Packets;
using NitroxServer.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

public class FootstepPacketProcessor : AuthenticatedPacketProcessor<FootstepPacket>
{
    private readonly float footstepAudioRange; // To modify this value, modify the last value of the event:/player/footstep_precursor_base sound in the SoundWhitelist_Subnautica.csv file
    private readonly PlayerManager playerManager;

    public FootstepPacketProcessor(PlayerManager playerManager, FmodWhitelist whitelist)
    {
        this.playerManager = playerManager;
        whitelist.TryGetSoundData("event:/player/footstep_precursor_base", out SoundData soundData);
        footstepAudioRange = soundData.Radius;
    }

    public override void Process(FootstepPacket footstepPacket, NitroxServer.Player sendingPlayer)
    {
        foreach (NitroxServer.Player player in playerManager.GetConnectedPlayers())
        {
            if (NitroxVector3.Distance(player.Position, sendingPlayer.Position) >= footstepAudioRange ||
                player == sendingPlayer)
            {
                continue;
            }
            if(player.SubRootId.Equals(sendingPlayer.SubRootId))
            {
                player.SendPacket(footstepPacket);
            }
        }
    }
}
