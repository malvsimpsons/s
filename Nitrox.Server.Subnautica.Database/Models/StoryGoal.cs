using System.ComponentModel.DataAnnotations.Schema;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Database.Models;

[Table("StoryGoals")]
public record StoryGoal
{
    public enum GoalPhase
    {
        NONE,
        RADIO_QUEUE,
        COMPLETED
    }

    public int Id { get; set; }

    /// <summary>
    ///     Key or name of the goal as known by Subnautica.
    /// </summary>
    public string GoalKey { get; set; }

    /// <summary>
    ///     Time in seconds of game world that this goal should trigger.
    /// </summary>
    public float ExecuteTime { get; set; }

    public Schedule.GoalCategory Category { get; set; }
    public GoalPhase Phase { get; set; }
}
