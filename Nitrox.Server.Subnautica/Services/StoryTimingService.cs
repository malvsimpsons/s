using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nitrox.Server.Subnautica.Models.Persistence;
using Nitrox.Server.Subnautica.Models.Persistence.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Keeps track of story timing and Aurora-related events.
/// </summary>
internal sealed class StoryTimingService(PlayerService playerService, TimeService timeService, IStateManager<PdaData> pda, IStateManager<StoryGoalData> storyGoals, IStateManager<StoryTimingData> storyTiming, ILogger<StoryTimingService> logger)
    : IHostedService
{
    public enum TimeModification
    {
        DAY, NIGHT, SKIP
    }

    private readonly ILogger<StoryTimingService> logger = logger;
    private readonly IStateManager<PdaData> pda = pda;
    private readonly PlayerService playerService = playerService;
    private readonly IStateManager<StoryGoalData> storyGoals = storyGoals;
    private readonly IStateManager<StoryTimingData> storyTiming = storyTiming;
    private readonly TimeService timeService = timeService;

    public void ReadjustAuroraRealExplosionTime(double skipSeconds)
    {
        // Readjust the aurora real explosion time when time skipping because it's based on in-game time
        if (storyTiming.State.AuroraRealExplosionTime > timeService.RealTimeElapsed)
        {
            double newTime = timeService.RealTimeElapsed.TotalSeconds + skipSeconds;
            if (newTime > storyTiming.State.AuroraRealExplosionTime.TotalSeconds)
            {
                storyTiming.State.AuroraRealExplosionTime = timeService.RealTimeElapsed;
            }
            else
            {
                storyTiming.State.AuroraRealExplosionTime -= TimeSpan.FromSeconds(skipSeconds);
            }
        }
    }

    /// <param name="instantaneous">Whether we should make Aurora explode instantly or after a short countdown</param>
    public void BroadcastExplodeAurora(bool instantaneous)
    {
        storyTiming.SetOrDefault(storyTiming.State with
        {
            // Calculations from CrashedShipExploder.OnConsoleCommand_countdownship()
            // We add 3 seconds to the cooldown (Subnautica adds only 1) so that players have enough time to receive the packet and process it
            AuroraCountdownStartTime = timeService.Elapsed.Add(TimeSpan.FromSeconds(3)),
            AuroraWarningStartTime = timeService.Elapsed.Add(TimeSpan.FromSeconds(3)),
            AuroraRealExplosionTime = storyTiming.State.AuroraRealExplosionTime.Add(TimeSpan.FromSeconds(3))
        });

        if (instantaneous)
        {
            // Calculations from CrashedShipExploder.OnConsoleCommand_explodeship()
            storyTiming.SetOrDefault(storyTiming.State with
            {
                // Removes 25 seconds to the countdown time, jumping to the exact moment of the explosion
                AuroraCountdownStartTime = storyTiming.State.AuroraCountdownStartTime.Subtract(TimeSpan.FromSeconds(25)),
                AuroraRealExplosionTime = storyTiming.State.AuroraRealExplosionTime.Subtract(TimeSpan.FromSeconds(25)),
                // Is 1 second less than countdown time to have the game understand that we only want the explosion.
                AuroraWarningStartTime = storyTiming.State.AuroraCountdownStartTime.Subtract(TimeSpan.FromSeconds(1))
            });
            logger.LogInformation("Aurora's explosion initiated");
        }
        else
        {
            logger.LogInformation("Aurora's explosion countdown will start in 3 seconds");
        }

        playerService.SendPacketToAllPlayers(new AuroraAndTimeUpdate(GetTimeData(), false));
    }

    public void BroadcastRestoreAurora()
    {
        storyTiming.SetOrDefault();

        // We need to clear these entries from PdaLog and CompletedGoals to make sure that the client, when reconnecting, doesn't have false information
        foreach (string eventKey in AuroraEventData.GoalNames)
        {
            pda.State.PdaLog.RemoveAllFast(eventKey, (entry, key) => entry.Key == key);
            storyGoals.State.CompletedGoals.Remove(eventKey);
        }

        playerService.SendPacketToAllPlayers(new AuroraAndTimeUpdate(GetTimeData(), true));
        logger.LogInformation("Restored Aurora, will explode again in {AuroraExplodeEta} minutes", GetMinutesBeforeAuroraExplosion());
    }

    /// <summary>
    ///     Clears the already completed sunbeam events to come and broadcasts it to all players along with the rescheduling of
    ///     the specified sunbeam event.
    /// </summary>
    public void StartSunbeamEvent(string sunbeamEventKey)
    {
        int beginIndex = PlaySunbeamEvent.SunbeamGoals.GetIndex(sunbeamEventKey);
        if (beginIndex == -1)
        {
            logger.LogError("Couldn't find the corresponding sunbeam event in {MemberName} for key {GoalName}", nameof(PlaySunbeamEvent.SunbeamGoals), sunbeamEventKey);
            return;
        }
        for (int i = beginIndex; i < PlaySunbeamEvent.SunbeamGoals.Length; i++)
        {
            storyGoals.State.CompletedGoals.Remove(PlaySunbeamEvent.SunbeamGoals[i]);
        }
        playerService.SendPacketToAllPlayers(new PlaySunbeamEvent(sunbeamEventKey));
    }

    /// <summary>
    ///     Makes a nice status of the Aurora events progress for the summary command.
    /// </summary>
    public string GetAuroraStateSummary()
    {
        double minutesBeforeExplosion = GetMinutesBeforeAuroraExplosion();
        if (minutesBeforeExplosion < 0)
        {
            return "already exploded";
        }
        // Based on AuroraWarnings.Update calculations
        // auroraWarningNumber is the amount of received Aurora warnings (there are 4 in total)
        int auroraWarningNumber = 0;
        (TimeSpan warning, TimeSpan countdown) = (storyTiming.State.AuroraWarningStartTime, storyTiming.State.AuroraCountdownStartTime);
        if (timeService.Elapsed >= countdown)
        {
            auroraWarningNumber = 4;
        }
        else if (timeService.Elapsed >= Mathf.Lerp(warning, countdown, 0.8f))
        {
            auroraWarningNumber = 3;
        }
        else if (timeService.Elapsed >= Mathf.Lerp(warning, countdown, 0.5f))
        {
            auroraWarningNumber = 2;
        }
        else if (timeService.Elapsed >= Mathf.Lerp(warning, countdown, 0.2f))
        {
            auroraWarningNumber = 1;
        }

        return $"explodes in {minutesBeforeExplosion} minutes [{auroraWarningNumber}/4]";
    }

    public AuroraEventData MakeAuroraData()
    {
        float countdownStartTime = (float)storyTiming.State.AuroraCountdownStartTime.TotalSeconds;
        float timeToStartWarning = (float)storyTiming.State.AuroraWarningStartTime.TotalSeconds;
        float auroraRealExplosionTime = (float)storyTiming.State.AuroraRealExplosionTime.TotalSeconds;
        return new AuroraEventData(countdownStartTime, timeToStartWarning, auroraRealExplosionTime);
    }

    public TimeData GetTimeData() => new(timeService.MakeTimePacket(), MakeAuroraData());

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Waits for data to be loaded that this service depends on.
        await Task.WhenAll(storyTiming.GetStateAsync(cancellationToken), storyGoals.GetStateAsync(cancellationToken), pda.GetStateAsync(cancellationToken));

        timeService.TimeSkipped += ReadjustAuroraRealExplosionTime;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        timeService.TimeSkipped -= ReadjustAuroraRealExplosionTime;
        return Task.CompletedTask;
    }

    /// <returns>Either the time in before Aurora explodes or -1 if it has already exploded.</returns>
    private double GetMinutesBeforeAuroraExplosion() => storyTiming.State.AuroraCountdownStartTime > timeService.Elapsed ? storyTiming.State.AuroraCountdownStartTime.Subtract(timeService.Elapsed).TotalMinutes : -1;
}
