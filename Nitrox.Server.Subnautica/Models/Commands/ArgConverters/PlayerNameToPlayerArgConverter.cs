using Nitrox.Server.Subnautica.Models.Commands.ArgConverters.Core;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Commands.ArgConverters;

/// <summary>
///     Converts a player name to a player object, if known.
/// </summary>
internal class PlayerNameToPlayerArgConverter(PlayerService playerService) : IArgConverter<string, NitroxServer.Player>
{
    private readonly PlayerService playerService = playerService;

    public ConvertResult Convert(string from)
    {
        NitroxServer.Player player = playerService.GetPlayer(from).OrNull();
        if (player == null)
        {
            return ConvertResult.Fail($"No player found by name '{from}'");
        }
        return ConvertResult.Ok(player);
    }
}
