using Nitrox.Server.Subnautica.Models.Events.Core;

namespace Nitrox.Server.Subnautica.Models.Events;

internal interface ISeeDbInitialized : IListen<ISeeDbInitialized, object>
{
    ValueTask HandleDatabaseInitialized();
    ValueTask IListen<ISeeDbInitialized, object>.HandleEvent(object context) => HandleDatabaseInitialized();
}
