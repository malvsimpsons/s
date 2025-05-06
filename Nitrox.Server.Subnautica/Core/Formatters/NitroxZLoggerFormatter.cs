using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging.Console;
using Nitrox.Server.Subnautica.Core.Configuration;

namespace Nitrox.Server.Subnautica.Core.Formatters;

public class NitroxZLoggerFormatter : IZLoggerFormatter
{
    private static volatile int emitAnsiColorCodes = -1;

    public bool WithLineBreak => false;

    private static bool EmitAnsiColorCodes
    {
        get
        {
            int emitAnsi = emitAnsiColorCodes;
            if (emitAnsi != -1)
            {
                return Convert.ToBoolean(emitAnsi);
            }

            bool enabled = !Console.IsOutputRedirected;
            if (enabled)
            {
                enabled = Environment.GetEnvironmentVariable("NO_COLOR") is null;
            }
            else
            {
                string envVar = Environment.GetEnvironmentVariable("DOTNET_SYSTEM_CONSOLE_ALLOW_ANSI_COLOR_REDIRECTION");
                enabled = envVar is not null && (envVar == "1" || envVar.Equals("true", StringComparison.OrdinalIgnoreCase));
            }

            emitAnsiColorCodes = Convert.ToInt32(enabled);
            return enabled;
        }
    }

    internal NitroxFormatterOptions FormatterOptions { get; init; }


    public void FormatLogEntry(IBufferWriter<byte> writer, IZLoggerEntry entry)
    {
        DateTimeOffset datetime = entry.LogInfo.Timestamp.Local;
        Span<byte> dateTimeDestination = writer.GetSpan();
        if (!datetime.TryFormat(dateTimeDestination, out int written, FormatterOptions.TimestampFormat))
        {
            datetime.TryFormat(dateTimeDestination, out written);
        }
        writer.Write(dateTimeDestination[..written]);

        ReadOnlySpan<byte> logLevelText = GetLogLevelText(entry.LogInfo.LogLevel);
        if (!logLevelText.IsEmpty)
        {
            ConsoleColors logLevelColors = GetLogLevelConsoleColors(entry.LogInfo.LogLevel);
            ConsoleColor? foreground = logLevelColors.Foreground;
            ConsoleColor? background = logLevelColors.Background;
            // Order: backgroundcolor, foregroundcolor, Message, reset foregroundcolor, reset backgroundcolor
            if (background.HasValue)
            {
                writer.Write(AnsiParser.GetBackgroundColorEscapeCode(background.Value));
            }
            if (foreground.HasValue)
            {
                writer.Write(AnsiParser.GetForegroundColorEscapeCode(foreground.Value));
            }
            writer.Write(logLevelText);
            if (foreground.HasValue)
            {
                writer.Write(AnsiParser.DefaultForegroundColor); // reset the foreground color
            }
            if (background.HasValue)
            {
                writer.Write(AnsiParser.DefaultBackgroundColor); // reset the background color
            }
        }

        // category - if type name, truncate namespace.
        writer.Write(" "u8);
        writer.Write(entry.LogInfo.Category.Utf8Span[(entry.LogInfo.Category.Utf8Span.LastIndexOf("."u8) + 1) ..]);
        writer.Write(": "u8);

        // scope information
        WriteScopeInformation(writer, entry.LogInfo.ScopeState);
        entry.ToString(writer);

        // Example:
        // System.InvalidOperationException
        //    at Namespace.Class.Function() in File:line X
        if (entry.LogInfo.Exception is { } exception)
        {
            // exception message
            writer.Write(exception.ToString());
        }
        writer.WriteLine();
    }

    private static ReadOnlySpan<byte> GetLogLevelText(LogLevel logLevel) =>
        logLevel switch
        {
            LogLevel.Trace => "[trce]"u8,
            LogLevel.Debug => "[dbug]"u8,
            LogLevel.Information => "[info]"u8,
            LogLevel.Warning => "[warn]"u8,
            LogLevel.Error => "[fail]"u8,
            LogLevel.Critical => "[crit]"u8,
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel))
        };

    private ConsoleColors GetLogLevelConsoleColors(LogLevel logLevel)
    {
        bool disableColors = FormatterOptions.ColorBehavior == LoggerColorBehavior.Disabled ||
                             (FormatterOptions.ColorBehavior == LoggerColorBehavior.Default && !EmitAnsiColorCodes);
        if (disableColors)
        {
            return new ConsoleColors(null, null);
        }
        return logLevel switch
        {
            LogLevel.Trace => new ConsoleColors(ConsoleColor.Gray, ConsoleColor.Black),
            LogLevel.Debug => new ConsoleColors(ConsoleColor.Gray, ConsoleColor.Black),
            LogLevel.Information => new ConsoleColors(ConsoleColor.DarkGreen, ConsoleColor.Black),
            LogLevel.Warning => new ConsoleColors(ConsoleColor.Yellow, ConsoleColor.Black),
            LogLevel.Error => new ConsoleColors(ConsoleColor.Black, ConsoleColor.DarkRed),
            LogLevel.Critical => new ConsoleColors(ConsoleColor.White, ConsoleColor.DarkRed),
            _ => new ConsoleColors(null, null)
        };
    }

    private void WriteScopeInformation(IBufferWriter<byte> writer, LogScopeState scopeProvider)
    {
        if (FormatterOptions.IncludeScopes && !scopeProvider.IsEmpty)
        {
            foreach (KeyValuePair<string, object> scopeProviderProperty in scopeProvider.Properties)
            {
                writer.Write(" => "u8);
                writer.Write(Encoding.UTF8.GetBytes(scopeProviderProperty.Value.ToString() ?? ""));
            }
        }
    }

    private readonly struct ConsoleColors
    {
        public ConsoleColors(ConsoleColor? foreground, ConsoleColor? background)
        {
            Foreground = foreground;
            Background = background;
        }

        public ConsoleColor? Foreground { get; }

        public ConsoleColor? Background { get; }
    }

    private static class AnsiParser
    {
        internal static ReadOnlySpan<byte> DefaultForegroundColor => "\e[39m\e[22m"u8; // reset to default foreground color
        internal static ReadOnlySpan<byte> DefaultBackgroundColor => "\e[49m"u8; // reset to the background color

        internal static ReadOnlySpan<byte> GetForegroundColorEscapeCode(ConsoleColor color) =>
            color switch
            {
                ConsoleColor.Black => "\e[30m"u8,
                ConsoleColor.DarkRed => "\e[31m"u8,
                ConsoleColor.DarkGreen => "\e[32m"u8,
                ConsoleColor.DarkYellow => "\e[33m"u8,
                ConsoleColor.DarkBlue => "\e[34m"u8,
                ConsoleColor.DarkMagenta => "\e[35m"u8,
                ConsoleColor.DarkCyan => "\e[36m"u8,
                ConsoleColor.Gray => "\e[37m"u8,
                ConsoleColor.Red => "\e[1m\e[31m"u8,
                ConsoleColor.Green => "\e[1m\e[32m"u8,
                ConsoleColor.Yellow => "\e[1m\e[33m"u8,
                ConsoleColor.Blue => "\e[1m\e[34m"u8,
                ConsoleColor.Magenta => "\e[1m\e[35m"u8,
                ConsoleColor.Cyan => "\e[1m\e[36m"u8,
                ConsoleColor.White => "\e[1m\e[37m"u8,
                _ => DefaultForegroundColor // default foreground color
            };

        internal static ReadOnlySpan<byte> GetBackgroundColorEscapeCode(ConsoleColor color) =>
            color switch
            {
                ConsoleColor.Black => "\e[40m"u8,
                ConsoleColor.DarkRed => "\e[41m"u8,
                ConsoleColor.DarkGreen => "\e[42m"u8,
                ConsoleColor.DarkYellow => "\e[43m"u8,
                ConsoleColor.DarkBlue => "\e[44m"u8,
                ConsoleColor.DarkMagenta => "\e[45m"u8,
                ConsoleColor.DarkCyan => "\e[46m"u8,
                ConsoleColor.Gray => "\e[47m"u8,
                _ => DefaultBackgroundColor // Use default background color
            };
    }
}
