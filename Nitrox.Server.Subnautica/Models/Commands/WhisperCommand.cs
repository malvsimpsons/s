using System.ComponentModel;
using Nitrox.Server.Subnautica.Models.Commands.Core;

namespace Nitrox.Server.Subnautica.Models.Commands;

[Alias("w", "msg", "m")]
internal class WhisperCommand : ICommandHandler<NitroxServer.Player, string>
{
    [Description("Sends a private message to a player")]
    public async Task Execute(ICommandContext context, [Description("The name of the player to message")] NitroxServer.Player receivingPlayer, [Description("The message to send")] string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        await context.MessageAsync(receivingPlayer.Id, $"[{context.OriginName} -> YOU]: {message}");
    }
}
