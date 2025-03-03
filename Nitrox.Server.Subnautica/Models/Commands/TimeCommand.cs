using System.ComponentModel;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.DataStructures.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.MODERATOR)]
internal class TimeCommand(TimeService timeService) : ICommandHandler<StoryTimingService.TimeModification>
{
    private readonly TimeService timeService = timeService;

    [Description("Changes the map time")]
    public void Execute(ICommandContext context, [Description("Time to change to")] StoryTimingService.TimeModification time)
    {
        switch (time)
        {
            case StoryTimingService.TimeModification.DAY:
                timeService.ChangeTime(StoryTimingService.TimeModification.DAY);
                context.MessageAll("Time set to day");
                break;
            case StoryTimingService.TimeModification.NIGHT:
                timeService.ChangeTime(StoryTimingService.TimeModification.NIGHT);
                context.MessageAll("Time set to night");
                break;
            case StoryTimingService.TimeModification.SKIP:
            default:
                timeService.ChangeTime(StoryTimingService.TimeModification.SKIP);
                context.MessageAll("Skipped time");
                break;
        }
    }
}
