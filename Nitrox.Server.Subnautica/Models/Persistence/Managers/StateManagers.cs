using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Nitrox.Server.Subnautica.Models.Configuration;
using Nitrox.Server.Subnautica.Models.Persistence.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Helper;
using NitroxServer.Helper;

namespace Nitrox.Server.Subnautica.Models.Persistence.Managers;

internal class StoryTimingStateManager(IOptions<SubnauticaServerOptions> optionsProvider) : IStateManager<StoryTimingData>
{
    /// <summary>
    ///     Initial game world time in Subnautica is 480s
    /// </summary>
    public const int INITIAL_TIME = 480;

    private readonly IOptions<SubnauticaServerOptions> optionsProvider = optionsProvider;

    public StoryTimingData State { get; set; }
    public TaskCompletionSource<object> CompletionSource { get; set; }

    public StoryTimingData CreateDefault()
    {
        StoryTimingData obj = new();
        obj.Elapsed = TimeSpan.FromSeconds(INITIAL_TIME); // TODO: Verify correct (-480 or +480)
        obj.AuroraCountdownStartTime = TimeSpan.FromMilliseconds(GenerateDeterministicAuroraTime(optionsProvider.Value.Seed));
        obj.AuroraWarningStartTime = default;
        // +27 is from CrashedShipExploder.IsExploded, -480 is from the default time in Subnautica.
        obj.AuroraRealExplosionTime = TimeSpan.FromSeconds(27).Subtract(TimeSpan.FromSeconds(INITIAL_TIME));
        return obj;
    }

    /// <summary>
    ///     Calculate the time at Aurora's explosion countdown will begin.
    /// </summary>
    private double GenerateDeterministicAuroraTime(string seed)
    {
        // Copied from CrashedShipExploder.SetExplodeTime() and changed from seconds to ms
        DeterministicGenerator generator = new(seed, nameof(StoryTimingService));
        return generator.NextDouble(2.3d, 4d) * 1200d * 1000d;
    }
}

internal class StoryGoalStateManager : IStateManager<StoryGoalData>
{
    public StoryGoalData State { get; set; }
    public TaskCompletionSource<object> CompletionSource { get; set; }
}

internal class PdaStateManager : IStateManager<PdaData>
{
    public TaskCompletionSource<object> CompletionSource { get; set; }
    public PdaData State { get; set; }
}

internal class EntityStateManager : IStateManager<EntityData>
{
    public TaskCompletionSource<object> CompletionSource { get; set; }
    public EntityData State { get; set; }
}

internal class PlayerStateManager : IStateManager<PlayerData>
{
    public TaskCompletionSource<object> CompletionSource { get; set; }
    public PlayerData State { get; set; }
}

internal class WorldStateManager : IStateManager<WorldData>
{
    public TaskCompletionSource<object> CompletionSource { get; set; }
    public WorldData State { get; set; }
}

internal class GlobalRootStateManager : IStateManager<GlobalRootData>
{
    public TaskCompletionSource<object> CompletionSource { get; set; }
    public GlobalRootData State { get; set; }
}

internal class SaveVersionStateManager : IStateManager<SaveVersionData>
{
    public string GroupKey => "Version";

    public SaveVersionData CreateDefault() => new(NitroxEnvironment.Version);

    public TaskCompletionSource<object> CompletionSource { get; set; }
    public SaveVersionData State { get; set; }
}
