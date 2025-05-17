using System;

namespace Nitrox.Server.Subnautica.Models.Commands.Core;

public class NopCommandSubmit : ICommandSubmit
{
    public void ExecuteCommand(ReadOnlySpan<char> inputText, ICommandContext context)
    {
        // Do nothing
    }
}
