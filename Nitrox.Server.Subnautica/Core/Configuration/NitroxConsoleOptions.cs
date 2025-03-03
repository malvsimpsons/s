using Microsoft.Extensions.Logging.Console;

namespace Nitrox.Server.Subnautica.Core.Configuration;

public class NitroxConsoleOptions : ConsoleFormatterOptions
{
    public NitroxConsoleOptions()
    {
        TimestampFormat = "[HH:mm:ss.fff] ";
    }

    public LoggerColorBehavior ColorBehavior { get; set; } = LoggerColorBehavior.Enabled;
    public bool IsDevMode { get; set; }
}
