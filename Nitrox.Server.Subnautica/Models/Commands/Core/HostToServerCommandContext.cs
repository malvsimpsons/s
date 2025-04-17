using Microsoft.Extensions.Logging;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;

namespace Nitrox.Server.Subnautica.Models.Commands.Core;

internal sealed record HostToServerCommandContext : ICommandContext
{
    private readonly PlayerService playerService;

    public ILogger Logger { get; set; }
    public CommandOrigin Origin { get; init; } = CommandOrigin.SERVER;
    public string OriginName => "SERVER";
    public ushort OriginId { get; init; } = ChatMessage.SERVER_ID;
    public Perms Permissions { get; init; } = Perms.SUPERADMIN;

    public HostToServerCommandContext(PlayerService playerService)
    {
        this.playerService = playerService;
    }

    public void Message(ushort id, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }
        if (!playerService.TryGetPlayerById(id, out NitroxServer.Player player))
        {
            Logger.LogWarning("No player found with id {PlayerId}", id);
            return;
        }
        player.SendPacket(new ChatMessage(OriginId, message));
    }

    public void Reply(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }
        Logger.LogInformation(message);
    }

    public void MessageAll(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }
        playerService.SendPacketToAllPlayers(new ChatMessage(OriginId, message));
        Reply($"[BROADCAST] {message}");
    }
}
