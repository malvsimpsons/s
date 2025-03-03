using Nitrox.Server.Subnautica.Models.Commands.ArgConverters.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxPlayer = NitroxServer.Player;

namespace Nitrox.Server.Subnautica.Models.Commands.ArgConverters;

/// <summary>
///     Converts a player ID to a player object, if known.
/// </summary>
internal class PlayerIdToPlayerArgConverter(PlayerService playerService) : IArgConverter<ushort, NitroxServer.Player>
{
    private readonly PlayerService playerService = playerService;

    public ConvertResult Convert(ushort playerId)
    {
        if (!playerService.TryGetPlayerById(playerId, out NitroxPlayer player))
        {
            return ConvertResult.Fail($"No player found with ID {playerId}");
        }

        return ConvertResult.Ok(player);
    }
}
