using System;

namespace Nitrox.Server.Subnautica.Models.Commands.Core;

public interface ICommandSubmit
{
    void ExecuteCommand(ReadOnlySpan<char> inputText, ICommandContext context);
}
