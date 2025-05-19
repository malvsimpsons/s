using System.Collections.Generic;
using System.Linq;
using NitroxClient.GameLogic.HUD;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel;
using NitroxClient.MonoBehaviours.Discord;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.GameLogic.FMOD;
using NitroxModel.Helper;
using NitroxModel.Networking;
using NitroxModel.Networking.Session;
using UnityEngine;

namespace NitroxClient.GameLogic;

public class PlayerManager
{
    private readonly PlayerModelManager playerModelManager;
    private readonly PlayerVitalsManager playerVitalsManager;
    private readonly FmodWhitelist fmodWhitelist;
    private readonly Dictionary<SessionId, RemotePlayer> playersBySessionId = new();

    public OnCreateDelegate OnCreate;
    public OnRemoveDelegate OnRemove;

    public PlayerManager(PlayerModelManager playerModelManager, PlayerVitalsManager playerVitalsManager, FmodWhitelist fmodWhitelist)
    {
        this.playerModelManager = playerModelManager;
        this.playerVitalsManager = playerVitalsManager;
        this.fmodWhitelist = fmodWhitelist;
    }

    public Optional<RemotePlayer> Find(SessionId sessionId)
    {
        playersBySessionId.TryGetValue(sessionId, out RemotePlayer player);
        return Optional.OfNullable(player);
    }

    public bool TryFind(SessionId playerId, out RemotePlayer remotePlayer) => playersBySessionId.TryGetValue(playerId, out remotePlayer);

    public Optional<RemotePlayer> Find(NitroxId playerNitroxId)
    {
        RemotePlayer remotePlayer = playersBySessionId.Select(idToPlayer => idToPlayer.Value)
                                               .FirstOrDefault(player => player.PlayerContext.PlayerNitroxId == playerNitroxId);

        return Optional.OfNullable(remotePlayer);
    }

    public IEnumerable<RemotePlayer> GetAll()
    {
        return playersBySessionId.Values;
    }

    public HashSet<GameObject> GetAllPlayerObjects()
    {
        HashSet<GameObject> remotePlayerObjects = GetAll().Select(player => player.Body).ToSet();
        remotePlayerObjects.Add(Player.mainObject);
        return remotePlayerObjects;
    }

    public RemotePlayer Create(PlayerContext playerContext)
    {
        Validate.NotNull(playerContext);
        Validate.IsFalse(playersBySessionId.ContainsKey(playerContext.PlayerId));

            RemotePlayer remotePlayer = new(playerContext, playerModelManager, playerVitalsManager, fmodWhitelist);
            
            playersBySessionId.Add(remotePlayer.PlayerId, remotePlayer);
            OnCreate(remotePlayer.PlayerId, remotePlayer);

        DiscordClient.UpdatePartySize(GetTotalPlayerCount());

        return remotePlayer;
    }

    public void RemovePlayer(SessionId sessionId)
    {
        if (playersBySessionId.TryGetValue(sessionId, out RemotePlayer player))
        {
            player.Destroy();
            playersBySessionId.Remove(sessionId);
            OnRemove(sessionId, player);
            DiscordClient.UpdatePartySize(GetTotalPlayerCount());
        }
    }

    /// <returns>Remote players + You => X + 1</returns>
    public int GetTotalPlayerCount() => playersBySessionId.Count + 1;

    public delegate void OnCreateDelegate(SessionId sessionId, RemotePlayer remotePlayer);
    public delegate void OnRemoveDelegate(SessionId sessionId, RemotePlayer remotePlayer);
}
