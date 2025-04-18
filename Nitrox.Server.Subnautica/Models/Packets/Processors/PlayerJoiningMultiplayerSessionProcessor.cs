using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using Nitrox.Server.Subnautica.Models.Configuration;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Bases;
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
using NitroxServer.Communication;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class PlayerJoiningMultiplayerSessionProcessor(
    StoryTimingService storyTimingService,
    PlayerService playerService,
    EntitySimulation entitySimulation,
    WorldEntityManager worldEntityManager,
    EscapePodService escapePodService,
    EntityRegistry entityRegistry,
    IOptions<SubnauticaServerOptions> optionsProvider,
    NtpSyncer ntpSyncer)
    : UnauthenticatedPacketProcessor<PlayerJoiningMultiplayerSession>
{
    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly EntitySimulation entitySimulation = entitySimulation;
    private readonly EscapePodService escapePodService = escapePodService;
    private readonly NtpSyncer ntpSyncer = ntpSyncer;
    private readonly PlayerService playerService = playerService;
    private readonly IOptions<SubnauticaServerOptions> optionsProvider = optionsProvider;
    private readonly StoryTimingService storyTimingService = storyTimingService;
    private readonly WorldEntityManager worldEntityManager = worldEntityManager;

    public override void Process(PlayerJoiningMultiplayerSession packet, INitroxConnection connection)
    {
        NitroxServer.Player player = playerService.AddConnectedPlayer(connection, packet.ReservationKey, out bool wasBrandNewPlayer);
        NitroxId assignedEscapePodId = escapePodService.AssignPlayerToEscapePod(player.Id, out Optional<EscapePodWorldEntity> newlyCreatedEscapePod);

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

        List<SimulatedEntity> simulations = entitySimulation.AssignGlobalRootEntitiesAndGetData(player);

        player.Entity = wasBrandNewPlayer ? SetupPlayerEntity(player) : RespawnExistingEntity(player);

        List<GlobalRootEntity> globalRootEntities = worldEntityManager.GetGlobalRootEntities(true);
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
                                                  new SubnauticaPlayerPreferences(new Dictionary<string, PingInstancePreference>(player.PingInstancePreferences), player.PinnedRecipePreferences.ToList()),
                                                  storyTimingService.GetTimeData(),
                                                  isFirstPlayer,
                                                  BuildingManager.GetEntitiesOperations(globalRootEntities),
                                                  optionsProvider.Value.KeepInventoryOnDeath
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

        PlayerWorldEntity playerEntity = new(transform, 0, null, false, player.GameObjectId, NitroxTechType.None, null, null, new List<Entity>());
        entityRegistry.AddOrUpdate(playerEntity);
        worldEntityManager.TrackEntityInTheWorld(playerEntity);
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
