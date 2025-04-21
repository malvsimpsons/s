using Nitrox.Server.Subnautica.Models.Commands.ArgConverters.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Dto;

namespace Nitrox.Server.Subnautica.Models.Commands.ArgConverters;

/// <summary>
///     Converts a player name to a player object, if known.
/// </summary>
internal class PlayerNameToPlayerArgConverter(PlayerService playerService) : IArgConverter<string, ConnectedPlayerDto>
{
    private readonly PlayerService playerService = playerService;

    public async Task<ConvertResult> ConvertAsync(string playerId)
    {
        ConnectedPlayerDto[] player = await playerService.GetConnectedPlayersByNameAsync(playerId);
        if (player == null)
        {
            return ConvertResult.Fail($"No player found by name '{playerId}'");
        }
        return ConvertResult.Ok(player);
    }
}
