using Nitrox.Server.Subnautica.Models.Commands.ArgConverters.Core;
using Nitrox.Server.Subnautica.Models.Respositories;
using NitroxModel.Dto;

namespace Nitrox.Server.Subnautica.Models.Commands.ArgConverters;

/// <summary>
///     Converts a player name to a player object, if known.
/// </summary>
internal class PlayerNameToPlayerArgConverter(PlayerRepository playerRepository) : IArgConverter<string, ConnectedPlayerDto>
{
    private readonly PlayerRepository playerRepository = playerRepository;

    public async Task<ConvertResult> ConvertAsync(string playerName)
    {
        ConnectedPlayerDto[] players = await playerRepository.GetConnectedPlayersByNameAsync(playerName);
        if (players == null || players.Length < 1)
        {
            return ConvertResult.Fail($"No player found by name '{playerName}'");
        }
        return ConvertResult.Ok(players[0]);
    }
}
