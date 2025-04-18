using System;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.DataStructures.Unity;
using NitroxModel.GameLogic.FMOD;
using NitroxModel.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class FootstepPacketProcessor(PlayerService playerService, FmodService fmodService) : AuthenticatedPacketProcessor<FootstepPacket>
{
    private float footstepAudioRange; // To modify this value, modify the last value of the event:/player/footstep_precursor_base sound in the SoundWhitelist_Subnautica.csv file
    private readonly PlayerService playerService = playerService;
    private readonly FmodService fmodService = fmodService;

    public override void Process(FootstepPacket footstepPacket, NitroxServer.Player sendingPlayer)
    {
        // TODO: Load this inside FmodService?
        fmodService.TryGetSoundData("event:/player/footstep_precursor_base", out SoundData soundData);
        if (soundData == null)
        {
            throw new Exception("Missing audio data for footsteps");
        }
        footstepAudioRange = soundData.Radius;

        foreach (NitroxServer.Player player in playerService.GetConnectedPlayers())
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
