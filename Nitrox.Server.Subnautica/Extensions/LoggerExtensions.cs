using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Nitrox.Server.Subnautica.Extensions;

public static class LoggerExtensions
{
    private static readonly HashSet<int> logOnceCache = [];

    public static void LogErrorOnce(this ILogger logger, string message, params object[] args)
    {
        if (!logOnceCache.Add(HashCode.Combine(message, args.Select(a => a.GetHashCode()))))
        {
            return;
        }
        logger.LogError(message, args);
    }
}
