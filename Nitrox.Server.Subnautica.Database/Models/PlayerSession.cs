using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NitroxModel.Networking;

namespace Nitrox.Server.Subnautica.Database.Models;

// TODO: TRUNCATE TABLE ON SERVER STARTUP

/// <summary>
///     The active sessions table. Deleting a session will also purge all session data (FOREIGN KEY CASCADE DELETE).
/// </summary>
/// <remarks>
///     On startup, this table is truncated.
/// </remarks>
[Table("PlayerSession")]
public record PlayerSession
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public SessionId Id { get; set; }

    /// <summary>
    ///     Gets or sets the connection attached to this session. Can be NULL.
    /// </summary>
    /// <remarks>
    ///     <b>Is <c>NULL</c> if</b>:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Client disconnected but session is still being migrated.</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    public Connection Connection { get; set; }

    /// <summary>
    ///     Gets the player reference. Can be <c>NULL</c>.
    /// </summary>
    /// <remarks>
    ///     <b>Is <c>NULL</c> if</b>:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Client is not playing yet but is connected. Like when entering player name or server password.</description>
    ///         </item>
    ///     </list>
    ///     If client disconnects, this session will (soon) be destroyed. Allowing
    ///     the same player data to be reused <i>with proper authentication</i>.
    /// </remarks>
    public Player Player { get; set; }
}
