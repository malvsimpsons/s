using Nitrox.Server.Subnautica.Models.Events.Core;

namespace Nitrox.Server.Subnautica.Models.Events;

internal interface ISeeResume : IParallelListen<ISeeResume, object>
{
    ValueTask Resume();
    ValueTask IListen<ISeeResume, object>.HandleEvent(object context) => Resume();
}
