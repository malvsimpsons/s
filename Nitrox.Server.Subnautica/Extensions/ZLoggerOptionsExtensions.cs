using System;
using Nitrox.Server.Subnautica.Core.Configuration;
using Nitrox.Server.Subnautica.Core.Formatters;

namespace Nitrox.Server.Subnautica.Extensions;

public static class ZLoggerOptionsExtensions
{
    public static T UseNitroxFormatter<T>(this T options, Action<NitroxFormatterOptions> configure = null) where T : ZLoggerOptions
    {
        options.UseFormatter(() =>
        {
            NitroxFormatterOptions formatterOptions = new();
            configure?.Invoke(formatterOptions);
            return new NitroxZLoggerFormatter { FormatterOptions = formatterOptions };
        });
        return options;
    }
}
