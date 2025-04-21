using System.ComponentModel;
using System.Text;
using Nitrox.Server.Subnautica.Models.Commands.Core;

namespace Nitrox.Server.Subnautica.Models.Commands;

internal class WhoisCommand : ICommandHandler<NitroxServer.Player>
{
    [Description("Shows information about a player")]
    public Task Execute(ICommandContext context, [Description("The players name")] NitroxServer.Player player)
    {
        StringBuilder builder = new($"==== {player.Name} ====\n");
        builder.AppendLine($"ID: {player.Id}");
        builder.AppendLine($"Role: {player.Permissions}");
        builder.AppendLine($"Position: {player.Position.X}, {player.Position.Y}, {player.Position.Z}");
        builder.AppendLine($"Oxygen: {player.Stats.Oxygen}/{player.Stats.MaxOxygen}");
        builder.AppendLine($"Food: {player.Stats.Food}");
        builder.AppendLine($"Water: {player.Stats.Water}");
        builder.AppendLine($"Infection: {player.Stats.InfectionAmount}");

        context.Reply(builder.ToString());

        return Task.CompletedTask;
    }
}
