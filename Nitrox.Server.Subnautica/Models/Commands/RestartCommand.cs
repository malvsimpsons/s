using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nitrox.Server.Subnautica.Models.Commands.Core;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresOrigin(CommandOrigin.SERVER)]
internal sealed class RestartCommand(ILogger<RestartCommand> logger, IHostApplicationLifetime lifetime) : ICommandHandler
{
    private readonly IHostApplicationLifetime lifetime = lifetime;

    [Description("Restarts the server")]
    public void Execute(ICommandContext context)
    {
        if (Debugger.IsAttached)
        {
            logger.LogError("Server can not be restarted while a debugger is attached.");
            return;
        }

        using Process currentProcess = Process.GetCurrentProcess();
        string program = currentProcess.MainModule?.FileName;
        if (program == null)
        {
            logger.LogError("Failed to get location of server.");
            return;
        }

        context.MessageAll("Server is restarting...");

        // TODO: STOP AND START SERVER
        lifetime.StopApplication();
        // using Process proc = Process.Start(program);
    }
}
