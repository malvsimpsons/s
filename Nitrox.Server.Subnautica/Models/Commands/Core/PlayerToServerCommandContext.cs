using System;
using Microsoft.Extensions.Logging;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Dto;
using NitroxModel.Networking;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Commands.Core;

internal sealed record PlayerToServerCommandContext : ICommandContext
{
    private readonly PlayerService playerService;
    public ILogger Logger { get; set; }
    public CommandOrigin Origin { get; init; } = CommandOrigin.PLAYER;
    public string OriginName => Player.Name;
    public PeerId OriginId { get; init; }
    public Perms Permissions { get; init; }

    /// <summary>
    ///     Gets the player which issued the command.
    /// </summary>
    public ConnectedPlayerDto Player { get; init; }

    public PlayerToServerCommandContext(PlayerService playerService, ConnectedPlayerDto player)
    {
        ArgumentNullException.ThrowIfNull(playerService);
        ArgumentNullException.ThrowIfNull(player);
        this.playerService = playerService;
        Player = player;
        OriginId = player.Id;
        Permissions = player.Permissions;
    }

    public async Task MessageAsync(PeerId id, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }
        if (OriginId == id)
        {
            Reply(message);
            return;
        }
        ConnectedPlayerDto player = await playerService.GetConnectedPlayerByIdAsync(id);
        if (player == null)
        {
            Logger.LogWarning("No player found with id {PlayerId}", id);
            return;
        }
        playerService.SendPacket(new ChatMessage(OriginId, message), id);
    }

    public void Reply(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }
        playerService.SendPacket(new ChatMessage(PeerId.SERVER_ID, message), OriginId);
    }

    public async Task MessageAllAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }
        playerService.SendPacketToOtherPlayers(new ChatMessage(PeerId.SERVER_ID, message), OriginId);
        Logger.LogInformation("Player {PlayerName} #{PlayerId} sent a message to everyone:{Message}", Player.Name, Player.Id, message);
    }
}
