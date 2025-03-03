using System;
using static NitroxModel.Packets.PlaySunbeamEvent;
using static NitroxModel.Packets.PlaySunbeamEvent.SunbeamEvent;

namespace NitroxModel.Subnautica.Extensions;

public static class SunbeamEventExtensions
{
    public static string ToSubnauticaStoryKey(this SunbeamEvent storyEvent) =>
        storyEvent switch
        {
            STORYSTART => "RadioSunbeamStart",
            GUNAIM => "PrecursorGunAimCheck",
            COUNTDOWN => "OnPlayRadioSunbeam4",
            _ => throw new ArgumentOutOfRangeException(nameof(storyEvent), $"Unknown {nameof(SunbeamEvent)} with number {(int)storyEvent}.")
        };
}
