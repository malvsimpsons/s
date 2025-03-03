using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Nitrox.Server.Subnautica.Models.Commands.Core;

namespace Nitrox.Server.Subnautica.Models.Commands;

[Alias("dir")]
[RequiresOrigin(CommandOrigin.SERVER)]
internal class DirectoryCommand(ILogger<DirectoryCommand> logger) : ICommandHandler
{
    [Description("Opens the current directory of the server")]
    public void Execute(ICommandContext context)
    {
        string path = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);

        if (!Directory.Exists(path))
        {
            logger.LogError("Unable to open Nitrox directory {Path} because it does not exist", path);
            return;
        }

        logger.LogInformation("Opening directory {Path}", path);
        Process.Start(new ProcessStartInfo(path) { UseShellExecute = true, Verb = "open" })?.Dispose();   
    }
}
