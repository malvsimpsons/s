using Microsoft.Extensions.Logging;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Dto;
using NitroxModel.Networking;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Commands.Core;

internal sealed record HostToServerCommandContext : ICommandContext
{
    private readonly PlayerService playerService;

    public ILogger Logger { get; set; }
    public CommandOrigin Origin { get; init; } = CommandOrigin.SERVER;
    public string OriginName => "SERVER";
    public PeerId OriginId { get; init; } = PeerId.SERVER_ID;
    public Perms Permissions { get; init; } = Perms.SUPERADMIN;

    public HostToServerCommandContext(PlayerService playerService)
    {
        this.playerService = playerService;
    }

    public async Task MessageAsync(PeerId id, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }
        ConnectedPlayerDto player = await playerService.GetConnectedPlayerByIdAsync(id);
        if (player == null)
        {
            Logger.LogWarning("No player found with id {PlayerId}", id);
            return;
        }
        playerService.SendPacketToOtherPlayers(new ChatMessage(id, message), OriginId);
    }

    public void Reply(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }
        Logger.LogInformation(message);
    }

    public async Task MessageAllAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }
        playerService.SendPacketToAllPlayers(new ChatMessage(OriginId, message));
        Reply($"[BROADCAST] {message}");
    }
}
