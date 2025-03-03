using System.Collections.Generic;
using System.Linq;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.MultiplayerSession;
using NitroxModel.Networking;
using NitroxModel.Packets;
using NitroxModel.Serialization;
using NitroxServer.Communication;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Bases;
using NitroxServer.GameLogic.Entities;
using NitroxServer.Serialization.World;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class PlayerJoiningMultiplayerSessionProcessor(ScheduleKeeper scheduleKeeper, StoryManager storyManager, PlayerService playerService, World world, EntityRegistry entityRegistry, SubnauticaServerConfig serverConfig, NtpSyncer ntpSyncer)
    : UnauthenticatedPacketProcessor<PlayerJoiningMultiplayerSession>
{
    private readonly PlayerService playerService = playerService;
    private readonly ScheduleKeeper scheduleKeeper = scheduleKeeper;
    private readonly StoryManager storyManager = storyManager;
    private readonly World world = world;
    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly SubnauticaServerConfig serverConfig = serverConfig;
    private readonly NtpSyncer ntpSyncer = ntpSyncer;

    public override void Process(PlayerJoiningMultiplayerSession packet, INitroxConnection connection)
    {
        NitroxServer.Player player = playerService.PlayerConnected(connection, packet.ReservationKey, out bool wasBrandNewPlayer);
        NitroxId assignedEscapePodId = world.EscapePodManager.AssignPlayerToEscapePod(player.Id, out Optional<EscapePodWorldEntity> newlyCreatedEscapePod);

        if (wasBrandNewPlayer)
        {
            player.SubRootId = assignedEscapePodId;
        }

        if (newlyCreatedEscapePod.HasValue)
        {
            SpawnEntities spawnNewEscapePod = new(newlyCreatedEscapePod.Value);
            playerService.SendPacketToOtherPlayers(spawnNewEscapePod, player);
        }

        // Make players on localhost admin by default.
        if (connection.Endpoint.Address.IsLocalhost())
        {
            Log.Info($"Granted admin to '{player.Name}' because they're playing on the host machine");
            player.Permissions = Perms.ADMIN;
        }

        List<SimulatedEntity> simulations = world.EntitySimulation.AssignGlobalRootEntitiesAndGetData(player);

        player.Entity = wasBrandNewPlayer ? SetupPlayerEntity(player) : RespawnExistingEntity(player);

        List<GlobalRootEntity> globalRootEntities = world.WorldEntityManager.GetGlobalRootEntities(true);
        bool isFirstPlayer = playerService.GetConnectedPlayers().Count == 1;

        InitialPlayerSync initialPlayerSync = new(player.GameObjectId,
                                                  wasBrandNewPlayer,
                                                  assignedEscapePodId,
                                                  player.EquippedItems,
                                                  player.UsedItems,
                                                  player.QuickSlotsBindingIds,
                                                  // TODO: FIX data here
                                                  default,
                                                  default,
                                                  // world.GameData.PDAState.GetInitialPDAData(),
                                                  // world.GameData.StoryGoals.GetInitialStoryGoalData(scheduleKeeper, player),
                                                  player.Position,
                                                  player.Rotation,
                                                  player.SubRootId,
                                                  player.Stats,
                                                  GetOtherPlayers(player),
                                                  globalRootEntities,
                                                  simulations,
                                                  player.GameMode,
                                                  player.Permissions,
                                                  wasBrandNewPlayer ? IntroCinematicMode.LOADING : IntroCinematicMode.COMPLETED,
                                                  new(new(player.PingInstancePreferences), player.PinnedRecipePreferences.ToList()),
                                                  storyManager.GetTimeData(),
                                                  isFirstPlayer,
                                                  BuildingManager.GetEntitiesOperations(globalRootEntities),
                                                  serverConfig.KeepInventoryOnDeath
        );

        player.SendPacket(initialPlayerSync);
    }

    private IEnumerable<PlayerContext> GetOtherPlayers(NitroxServer.Player player)
    {
        return playerService.GetConnectedPlayers().Where(p => p != player)
                            .Select(p => p.PlayerContext);
    }

    private PlayerWorldEntity SetupPlayerEntity(NitroxServer.Player player)
    {
        NitroxTransform transform = new(player.Position, player.Rotation, NitroxVector3.One);

        PlayerWorldEntity playerEntity = new PlayerWorldEntity(transform, 0, null, false, player.GameObjectId, NitroxTechType.None, null, null, new List<Entity>());
        entityRegistry.AddOrUpdate(playerEntity);
        world.WorldEntityManager.TrackEntityInTheWorld(playerEntity);
        return playerEntity;
    }

    private PlayerWorldEntity RespawnExistingEntity(NitroxServer.Player player)
    {
        if (entityRegistry.TryGetEntityById(player.PlayerContext.PlayerNitroxId, out PlayerWorldEntity playerWorldEntity))
        {
            return playerWorldEntity;
        }
        Log.Error($"Unable to find player entity for {player.Name}. Re-creating one");
        return SetupPlayerEntity(player);
    }
}
