using System;

namespace NitroxModel.Networking.Packets;

[Serializable]
public class Schedule(float timeExecute, string key, Schedule.GoalCategory category) : Packet
{
    /// <summary>
    ///     Same as GoalType in Subnautica.
    /// </summary>
    public enum GoalCategory
    {
        PDA,
        Radio,
        Encyclopedia,
        Story
    }

    public float TimeExecute { get; } = timeExecute;
    public string Key { get; } = key;
    public GoalCategory Category { get; } = category;
}
