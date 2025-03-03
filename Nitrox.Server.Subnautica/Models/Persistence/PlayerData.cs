using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using NitroxServer.GameLogic.Players;

namespace Nitrox.Server.Subnautica.Models.Persistence;

[DataContract]
public class PlayerData
{
    [DataMember(Order = 1)]
    public List<PersistedPlayerData> Players = [];

    public List<NitroxServer.Player> GetPlayers()
    {
        return Players.Select(playerData => playerData.ToPlayer()).ToList();
    }

    public static PlayerData From(IEnumerable<NitroxServer.Player> players)
    {
        List<PersistedPlayerData> persistedPlayers = players.Select(PersistedPlayerData.FromPlayer).ToList();

        return new PlayerData { Players = persistedPlayers };
    }
}
