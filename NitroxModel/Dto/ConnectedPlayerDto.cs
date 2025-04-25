using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Networking;

namespace NitroxModel.Dto;

/// <summary>
///     Session object used to reference a connected player.
/// </summary>
/// <remarks>
///     Take care that clients must not be able to authorize with this object.
/// </remarks>
public record ConnectedPlayerDto
{
    /// <summary>
    ///     Player ID, as known in database, assigned to this session object.
    /// </summary>
    public PeerId Id { get; init; }

    /// <summary>
    ///     Gets the auto incremented session ID of the connected player. This ID is not globally unique and should only be valid while the player is
    ///     connected.
    /// </summary>
    public SessionId SessionId { get; init; }

    /// <summary>
    ///     The name of the connected player.
    /// </summary>
    public string Name { get; init; }

    public Perms Permissions { get; init; }
}
