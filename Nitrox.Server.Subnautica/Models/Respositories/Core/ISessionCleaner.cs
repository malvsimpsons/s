using Nitrox.Server.Subnautica.Database.Models;

namespace Nitrox.Server.Subnautica.Models.Respositories.Core;

/// <summary>
///     Implementors migrate session data away from the disconnected session.
/// </summary>
public interface ISessionCleaner
{
    Task CleanSessionAsync(PlayerSession disconnectedSession);
}
