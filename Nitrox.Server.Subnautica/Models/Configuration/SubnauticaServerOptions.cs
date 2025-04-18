using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Serialization;
using NitroxModel.Server;

namespace Nitrox.Server.Subnautica.Models.Configuration;

/// <summary>
///     Options that are tied to a hosted game world.
/// </summary>
public sealed partial class SubnauticaServerOptions
{
    public const string CONFIG_SECTION_PATH = "GameServer";

    [Range(1, byte.MaxValue)]
    public byte MaxConnections { get; set; } = 100;

    [Range(1, ushort.MaxValue)]
    public short Port { get; set; } = 11000;

    [RegularExpression(@"\w+")]
    public string ServerPassword { get; set; } = "";

    public NitroxGameMode GameMode { get; set; }

    [Range(30001, int.MaxValue)]
    public int InitialSyncTimeout { get; set; } = 300000;

    public bool IsHardcore => GameMode == NitroxGameMode.HARDCORE;

    [PropertyDescription("Possible values:", typeof(Perms))]
    public Perms DefaultPlayerPerm { get; set; } = Perms.DEFAULT;

    public float DefaultOxygenValue { get; set; } = 45;

    public float DefaultMaxOxygenValue { get; set; } = 45;
    public float DefaultHealthValue { get; set; } = 80;
    public float DefaultHungerValue { get; set; } = 50.5f;
    public float DefaultThirstValue { get; set; } = 90.5f;

    [PropertyDescription("Recommended to keep at 0.1f which is the default starting value. If set to 0 then new players are cured by default.")]
    public float DefaultInfectionValue { get; set; } = 0.1f;

    public PlayerStatsData DefaultPlayerStats => new(DefaultOxygenValue, DefaultMaxOxygenValue, DefaultHealthValue, DefaultHungerValue, DefaultThirstValue, DefaultInfectionValue);

    [Required]
    [RegularExpression(@"\w+")]
    public string Seed { get; set; }

    public bool DisableConsole { get; set; }
    public string AdminPassword { get; set; }
    public bool KeepInventoryOnDeath { get; set; }
    public bool PvpEnabled { get; set; } = true;

    [OptionsValidator]
    internal partial class Validator : IValidateOptions<SubnauticaServerOptions>;
}
