using System.ComponentModel.DataAnnotations.Schema;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Networking;
using NitroxModel.Server;

namespace Nitrox.Server.Subnautica.Database.Models;

/// <summary>
///     The player model that clients can assume on join.
/// </summary>
/// <remarks>
///     This model should not hold session data. Data here is kept even in-between games.
///     Use <see cref="PlayerSession" /> and dependant tables if data should be discarded when player leaves.
/// </remarks>
[Table("Players")]
public record Player
{
    /// <summary>
    ///     Primary key in the database.
    /// </summary>
    public PeerId Id { get; set; }

    /// <summary>
    ///     Name of the player as it was provided by the player on join.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Permissions as granted by the server. Defaults to <see cref="Perms.DEFAULT" />.
    /// </summary>
    public Perms Permissions { get; set; }

    /// <summary>
    ///     The game mode this player is playing in. Can be different for other players in the same world.
    /// </summary>
    public SubnauticaGameMode GameMode { get; set; }

    /// <summary>
    ///     If true, player will have to start over when they die.
    /// </summary>
    public bool IsPermaDeath { get; set; }

    // TODO: Store this
    // public List<NitroxTechType> UsedItems { get; set; } = [];
    // public Optional<NitroxId>[] QuickSlotsBindingIds { get; set; } = [];
    // public Dictionary<string, NitroxId> EquippedItems { get; set; } = [];
    // public NitroxVector3 SpawnPosition { get; set; }
    // public NitroxQuaternion SpawnRotation { get; set; }
    // /// <summary>
    // ///     Gets the survival stats of the player in the game world.
    // /// </summary>
    // public PlayerStats PlayerStats { get; set; }
    // /// <summary>
    // ///     Synchronization id of the player object in the Subnautica world.
    // /// </summary>
    // public NitroxId NitroxId { get; set; }
    // /// <summary>
    // ///     Synchronization id of the structure the player has entered.
    // /// </summary>
    // /// <remarks>
    // ///     Subnautica uses <c>SubRoot</c> terminology for any interior the player can enter (like cyclops, seamoth or player
    // ///     bases).
    // /// </remarks>
    // public NitroxId SubRootId { get; set; }
}
