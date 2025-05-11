using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nitrox.Server.Subnautica.Database.Models;

/// <summary>
///     An active connection to the server.
/// </summary>
[Table("Connections")]
public record Connection
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    ///     Gets the endpoint address of the client. Can be IPv4 or IPv6.
    /// </summary>
    [Required]
    public string Address { get; set; }

    /// <summary>
    ///     Gets the port of the client.
    /// </summary>
    [Required]
    public ushort Port { get; set; }

    /// <summary>
    ///     The real-world time when the session was made.
    /// </summary>
    [Required]
    public DateTimeOffset Created { get; set; } = DateTimeOffset.Now;
}
