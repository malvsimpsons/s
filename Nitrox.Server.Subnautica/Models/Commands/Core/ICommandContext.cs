using Microsoft.Extensions.Logging;
using NitroxModel.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Networking;

namespace Nitrox.Server.Subnautica.Models.Commands.Core;

/// <summary>
///     A context that can be provided to a command as it is called.
/// </summary>
public interface ICommandContext
{
    public ILogger Logger { get; set; }

    CommandOrigin Origin { get; init; }

    public string OriginName { get; }

    /// <summary>
    ///     The id of the user that issued this command. 0 if server. Otherwise, it's the player ID as known in the database.
    /// </summary>
    PeerId OriginId { get; init; }

    /// <summary>
    ///     The permissions of the issuer as they were when the command was issued.
    /// </summary>
    Perms Permissions { get; init; }

    /// <summary>
    ///     Sends a message back to the command issuer.
    /// </summary>
    /// <param name="message">The message to send.</param>
    void Reply(string message);

    /// <summary>
    ///     Sends a message to the user id. Does nothing if user id is not found.
    /// </summary>
    /// <param name="id">The id of the receiving user.</param>
    /// <param name="message">The message to send.</param>
    Task MessageAsync(PeerId id, string message);

    /// <summary>
    ///     Sends a message to all other users.
    /// </summary>
    /// <param name="message">The message to send to all users.</param>
    Task MessageAllAsync(string message);
}
