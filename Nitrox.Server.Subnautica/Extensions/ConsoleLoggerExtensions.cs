using System;
using Microsoft.Extensions.Logging;
using Nitrox.Server.Subnautica.Core.Configuration;
using Nitrox.Server.Subnautica.Core.Formatters;

namespace Nitrox.Server.Subnautica.Extensions;

public static class ConsoleLoggerExtensions
{
    public static ILoggingBuilder AddNitroxConsole(
        this ILoggingBuilder builder,
        Action<NitroxConsoleOptions> configure = null) => builder
                                                          .AddConsole(options => options.FormatterName = NitroxConsoleFormatter.NAME)
                                                          .AddConsoleFormatter<NitroxConsoleFormatter, NitroxConsoleOptions>(configure ?? (_ => { }));
}
