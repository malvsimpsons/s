using Nitrox.Server.Subnautica.Database.Models;
using Nitrox.Server.Subnautica.Models.Events.Core;

namespace Nitrox.Server.Subnautica.Models.Events;

/// <summary>
///     Implementors migrate session data away from a disconnected session.
/// </summary>
internal interface ISeeSessionDisconnected : IOrderedListen<ISeeSessionDisconnected, Session>
{
    public ValueTask HandleSessionDisconnect(Session disconnectedSession);
    ValueTask IListen<ISeeSessionDisconnected, Session>.HandleEvent(Session context) => HandleSessionDisconnect(context);
}
