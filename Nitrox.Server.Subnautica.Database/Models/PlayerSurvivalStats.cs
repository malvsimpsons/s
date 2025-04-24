using System.ComponentModel.DataAnnotations.Schema;

namespace Nitrox.Server.Subnautica.Database.Models;

[Table("PlayerSurvivalStats")]
public record PlayerSurvivalStats
{
    [ForeignKey(nameof(Models.Player.Id))]
    public Player Player { get; set; }
    public float Oxygen { get; set; }
    public float MaxOxygen { get; set; }
    public float Health { get; set; }
    public float Food { get; set; }
    public float Water { get; set; }
    public float InfectionAmount { get; set; }
}
