using System;
using NitroxModel.Server;

namespace NitroxModel.Networking.Packets;

[Serializable]
public class GameModeChanged : Packet
{
    public PeerId PlayerId { get; }
    public bool AllPlayers { get; }
    public SubnauticaGameMode GameMode { get; }

    public GameModeChanged(PeerId playerId, bool allPlayers, SubnauticaGameMode gameMode)
    {
        PlayerId = playerId;
        AllPlayers = allPlayers;
        GameMode = gameMode;
    }

    public static GameModeChanged ForPlayer(PeerId playerId, SubnauticaGameMode gameMode)
    {
        return new(playerId, false, gameMode);
    }

    public static GameModeChanged ForAllPlayers(SubnauticaGameMode gameMode)
    {
        return new(0, true, gameMode);
    }
}
