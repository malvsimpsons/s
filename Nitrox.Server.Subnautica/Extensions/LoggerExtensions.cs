using Nitrox.Server.Subnautica.Core.Redaction;

namespace Nitrox.Server.Subnautica.Extensions;

internal static partial class LoggerExtensions
{
    [ZLoggerMessage(Level = LogLevel.Information, Message = "Using game files from {path}")]
    public static partial void LogGameInstallPathUsage(this ILogger logger, SensitiveData<string> path);
}
