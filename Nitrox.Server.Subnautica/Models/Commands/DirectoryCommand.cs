using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Nitrox.Server.Subnautica.Models.Commands.Core;

namespace Nitrox.Server.Subnautica.Models.Commands;

[Alias("dir")]
[RequiresOrigin(CommandOrigin.SERVER)]
internal class DirectoryCommand(ILogger<DirectoryCommand> logger) : ICommandHandler
{
    [Description("Opens the current directory of the server")]
    public Task Execute(ICommandContext context)
    {
        string path = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);

        if (!Directory.Exists(path))
        {
            logger.ZLogError($"Unable to open Nitrox directory {path} because it does not exist");
            return Task.CompletedTask;
        }

        logger.ZLogInformation($"Opening directory {path}");
        Process.Start(new ProcessStartInfo(path) { UseShellExecute = true, Verb = "open" })?.Dispose();
        return Task.CompletedTask;
    }
}
