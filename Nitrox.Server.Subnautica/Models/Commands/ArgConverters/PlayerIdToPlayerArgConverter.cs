using Nitrox.Server.Subnautica.Models.Commands.ArgConverters.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Core;
using NitroxModel.Dto;

namespace Nitrox.Server.Subnautica.Models.Commands.ArgConverters;

/// <summary>
///     Converts a player ID to a player object, if known.
/// </summary>
internal class PlayerIdToPlayerArgConverter(PlayerService playerService) : IArgConverter<ushort, ConnectedPlayerDto>
{
    private readonly PlayerService playerService = playerService;

    public async Task<ConvertResult> ConvertAsync(ushort playerId)
    {
        ConnectedPlayerDto player = await playerService.GetConnectedPlayerByIdAsync(playerId);
        if (player == null)
        {
            return ConvertResult.Fail($"No player found with ID {playerId}");
        }

        return ConvertResult.Ok(player);
    }
}
