using Microsoft.Extensions.Logging.Console;

namespace Nitrox.Server.Subnautica.Core.Configuration;

public class NitroxFormatterOptions : ConsoleFormatterOptions
{
    public NitroxFormatterOptions()
    {
        TimestampFormat = "[HH:mm:ss.fff] ";
    }

    public LoggerColorBehavior ColorBehavior { get; set; } = LoggerColorBehavior.Enabled;
}
