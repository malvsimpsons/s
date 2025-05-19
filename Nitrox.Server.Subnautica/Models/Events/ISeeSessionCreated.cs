using Nitrox.Server.Subnautica.Database.Models;
using Nitrox.Server.Subnautica.Models.Events.Core;

namespace Nitrox.Server.Subnautica.Models.Events;

internal interface ISeeSessionCreated : IOrderedListen<ISeeSessionCreated, Session>
{
    public ValueTask HandleSessionCreated(Session createdSession);
    ValueTask IListen<ISeeSessionCreated, Session>.HandleEvent(Session context) => HandleSessionCreated(context);
}
