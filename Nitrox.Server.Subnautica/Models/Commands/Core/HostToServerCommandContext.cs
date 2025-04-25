using System;
using Microsoft.Extensions.Logging;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.Respositories;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Dto;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Commands.Core;

internal sealed record HostToServerCommandContext : ICommandContext
{
    private readonly IServerPacketSender packetSender;
    private readonly PlayerRepository playerRepository;

    public ILogger Logger { get; set; }
    public CommandOrigin Origin { get; init; } = CommandOrigin.SERVER;
    public string OriginName => "SERVER";
    public SessionId OriginId { get; init; } = 0;
    public Perms Permissions { get; init; } = Perms.SUPERADMIN;

    public HostToServerCommandContext(PlayerRepository playerRepository, IServerPacketSender packetSender)
    {
        this.playerRepository = playerRepository;
        this.packetSender = packetSender;
    }

    public async Task MessageAsync(PeerId id, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }
        ConnectedPlayerDto player = await playerRepository.GetConnectedPlayerByPlayerIdAsync(id);
        if (player == null)
        {
            Logger.LogWarning("No player found with id {PlayerId}", id);
            return;
        }
        await SendAsync(new ChatMessage(player.SessionId, message), OriginId);
    }

    public Task ReplyAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return Task.CompletedTask;
        }
        Logger.LogInformation(message);
        return Task.CompletedTask;
    }

    public async Task MessageAllAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }
        await packetSender.SendPacketToAll(new ChatMessage(OriginId, message));
        await ReplyAsync($"[BROADCAST] {message}");
    }

    public async ValueTask SendAsync<T>(T data, PeerId peerId)
    {
        switch (data)
        {
            case Packet packet:
                await packetSender.SendPacket(packet, peerId);
                break;
            default:
                throw new NotSupportedException($"Unsupported data type {data?.GetType()}");
        }
    }

    public async ValueTask SendToAll<T>(T data)
    {
        switch (data)
        {
            case Packet packet:
                await packetSender.SendPacketToAll(packet);
                break;
            default:
                throw new NotSupportedException($"Unsupported data type {data?.GetType()}");
        }
    }

    public async ValueTask SendAsync<T>(T data, SessionId sessionId)
    {
        switch (data)
        {
            case Packet packet:
                await packetSender.SendPacket(packet, sessionId);
                break;
            default:
                throw new NotSupportedException($"Unsupported data type {data?.GetType()}");
        }
    }
}
