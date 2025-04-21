using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NitroxModel.Networking;

namespace Nitrox.Server.Subnautica.Database.Models;

[Table("PlayerSessions")]
public record PlayerSession
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public SessionId? SessionId { get; set; }

    public Player Player { get; set; }
}
