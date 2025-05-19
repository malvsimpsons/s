using System.ComponentModel;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using NitroxModel.DataStructures.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.MODERATOR)]
internal class SummaryCommand : ICommandHandler
{
    [Description("Shows persisted data")]
    public Task Execute(ICommandContext context)
    {
        // TODO: Fix save summary
        // Note for later additions: order these lines by their length
        // StringBuilder builder = new("\n");
        // if (viewerPerms is Perms.SUPERADMIN)
        // {
        //     builder.AppendLine($" - Save location: {Path.Combine(KeyValueStore.Instance.GetServerSavesPath(), Name)}");
        // }
        // //         builder.AppendLine($"""
        // //          - Aurora's state: {world.StoryManager.GetAuroraStateSummary()}
        // //          - Current time: day {world.TimeKeeper.Day} ({Math.Floor(world.TimeKeeper.ElapsedSeconds)}s)
        // //          - Scheduled goals stored: {world.GameData.StoryGoals.ScheduledGoals.Count}
        // //          - Story goals completed: {world.GameData.StoryGoals.CompletedGoals.Count}
        // //          - Radio messages stored: {world.GameData.StoryGoals.RadioQueue.Count}
        // //          - World gamemode: {serverConfig.GameMode}
        // //          - Encyclopedia entries: {world.GameData.PDAState.EncyclopediaEntries.Count}
        // //          - Known tech: {world.GameData.PDAState.KnownTechTypes.Count}
        // //         """);
        // return builder.ToString();

        return Task.CompletedTask;
    }
}
