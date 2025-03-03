using System;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using Nitrox.Server.Subnautica.Core.Configuration;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Nitrox.Server.Subnautica.Core.Formatters;

public class NitroxConsoleFormatter : ConsoleFormatter, IDisposable
{
    public const string NAME = "nitroxConsoleFormatter";

    private static volatile int emitAnsiColorCodes = -1;

    private readonly IDisposable optionsReloadToken;
    internal NitroxConsoleOptions FormatterOptions { get; set; }

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

    public NitroxConsoleFormatter(IOptionsMonitor<NitroxConsoleOptions> options) : base(NAME)
    {
        ReloadLoggerOptions(options.CurrentValue);
        optionsReloadToken = options.OnChange(ReloadLoggerOptions);
    }

    public void Dispose() => optionsReloadToken?.Dispose();

    public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider scopeProvider, TextWriter textWriter)
    {
        if (logEntry.State is BufferedLogRecord bufferedRecord)
        {
            string message = bufferedRecord.FormattedMessage ?? string.Empty;
            WriteInternal(null, textWriter, message, bufferedRecord.LogLevel, bufferedRecord.EventId.Id, bufferedRecord.Exception, logEntry.Category, bufferedRecord.Timestamp);
        }
        else
        {
            string message = logEntry.Formatter(logEntry.State, logEntry.Exception);
            if (logEntry.Exception == null && message == null)
            {
                return;
            }

            WriteInternal(scopeProvider, textWriter, message, logEntry.LogLevel, logEntry.EventId.Id, logEntry.Exception?.ToString(), logEntry.Category, GetCurrentDateTime());
        }
    }

    private static void WriteMessage(TextWriter textWriter, string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return;
        }
        textWriter.Write(' ');
        textWriter.Write(message);
    }

    private static string GetLogLevelString(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => "[trce]",
            LogLevel.Debug => "[dbug]",
            LogLevel.Information => "[info]",
            LogLevel.Warning => "[warn]",
            LogLevel.Error => "[fail]",
            LogLevel.Critical => "[crit]",
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel))
        };
    }

    private void ReloadLoggerOptions(NitroxConsoleOptions options) => FormatterOptions = options;

    private void WriteInternal(IExternalScopeProvider scopeProvider, TextWriter textWriter, string message, LogLevel logLevel,
                               int eventId, string exception, string category, DateTimeOffset stamp)
    {
        ConsoleColors logLevelColors = GetLogLevelConsoleColors(logLevel);
        string logLevelString = GetLogLevelString(logLevel);

        string timestamp = null;
        string timestampFormat = FormatterOptions.TimestampFormat;
        if (timestampFormat != null)
        {
            timestamp = stamp.ToString(timestampFormat);
        }
        if (timestamp != null)
        {
            textWriter.Write(timestamp);
        }
        if (logLevelString != null)
        {
            ConsoleColor? foreground = logLevelColors.Foreground;
            ConsoleColor? background = logLevelColors.Background;
            // Order: backgroundcolor, foregroundcolor, Message, reset foregroundcolor, reset backgroundcolor
            if (background.HasValue)
            {
                textWriter.Write(AnsiParser.GetBackgroundColorEscapeCode(background.Value));
            }
            if (foreground.HasValue)
            {
                textWriter.Write(AnsiParser.GetForegroundColorEscapeCode(foreground.Value));
            }
            textWriter.Write(logLevelString);
            if (foreground.HasValue)
            {
                textWriter.Write(AnsiParser.DefaultForegroundColor); // reset to default foreground color
            }
            if (background.HasValue)
            {
                textWriter.Write(AnsiParser.DefaultBackgroundColor); // reset to the background color
            }
        }

        // category and event id
        if (FormatterOptions.IsDevMode)
        {
            textWriter.Write(' ');
            textWriter.Write(category.AsSpan()[(category.LastIndexOf('.') + 1) ..]);
            textWriter.Write(':');

            // Event id
            // textWriter.Write('[');
            // Span<char> span = stackalloc char[10];
            // if (eventId.TryFormat(span, out int charsWritten))
            // {
            //     textWriter.Write(span[..charsWritten]);
            // }
            // else
            // {
            //     textWriter.Write(eventId.ToString());
            // }
            // textWriter.Write(']');
        }

        // scope information
        WriteScopeInformation(textWriter, scopeProvider);
        WriteMessage(textWriter, message);

        // Example:
        // System.InvalidOperationException
        //    at Namespace.Class.Function() in File:line X
        if (exception != null)
        {
            // exception message
            WriteMessage(textWriter, exception);
        }
        textWriter.Write(Environment.NewLine);
    }

    private DateTimeOffset GetCurrentDateTime()
    {
        return FormatterOptions.TimestampFormat != null
            ? FormatterOptions.UseUtcTimestamp ? DateTimeOffset.UtcNow : DateTimeOffset.Now
            : DateTimeOffset.MinValue;
    }

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

    private void WriteScopeInformation(TextWriter textWriter, IExternalScopeProvider scopeProvider)
    {
        if (FormatterOptions.IncludeScopes && scopeProvider != null)
        {
            scopeProvider.ForEachScope((scope, state) =>
            {
                state.Write(" => ");
                state.Write(scope);
            }, textWriter);
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

    internal sealed class AnsiParser
    {
        internal const string DefaultForegroundColor = "\e[39m\e[22m"; // reset to default foreground color
        internal const string DefaultBackgroundColor = "\e[49m"; // reset to the background color
        private readonly Action<string, int, int, ConsoleColor?, ConsoleColor?> _onParseWrite;

        public AnsiParser(Action<string, int, int, ConsoleColor?, ConsoleColor?> onParseWrite)
        {
            ArgumentNullException.ThrowIfNull(onParseWrite);

            _onParseWrite = onParseWrite;
        }

        /// <summary>
        ///     Parses a subset of display attributes
        ///     Set Display Attributes
        ///     Set Attribute Mode [{attr1};...;{attrn}m
        ///     Sets multiple display attribute settings. The following lists standard attributes that are getting parsed:
        ///     1 Bright
        ///     Foreground Colours
        ///     30 Black
        ///     31 Red
        ///     32 Green
        ///     33 Yellow
        ///     34 Blue
        ///     35 Magenta
        ///     36 Cyan
        ///     37 White
        ///     Background Colours
        ///     40 Black
        ///     41 Red
        ///     42 Green
        ///     43 Yellow
        ///     44 Blue
        ///     45 Magenta
        ///     46 Cyan
        ///     47 White
        /// </summary>
        public void Parse(string message)
        {
            int startIndex = -1;
            int length = 0;
            int escapeCode;
            ConsoleColor? foreground = null;
            ConsoleColor? background = null;
            ReadOnlySpan<char> span = message.AsSpan();
            const char EscapeChar = '\e';
            ConsoleColor? color = null;
            bool isBright = false;
            for (int i = 0; i < span.Length; i++)
            {
                if (span[i] == EscapeChar && span.Length >= i + 4 && span[i + 1] == '[')
                {
                    if (span[i + 3] == 'm')
                    {
                        // Example: \e[1m
                        if (IsDigit(span[i + 2]))
                        {
                            escapeCode = span[i + 2] - '0';
                            if (startIndex != -1)
                            {
                                _onParseWrite(message, startIndex, length, background, foreground);
                                startIndex = -1;
                                length = 0;
                            }
                            if (escapeCode == 1)
                            {
                                isBright = true;
                            }
                            i += 3;
                            continue;
                        }
                    }
                    else if (span.Length >= i + 5 && span[i + 4] == 'm')
                    {
                        // Example: \e[40m
                        if (IsDigit(span[i + 2]) && IsDigit(span[i + 3]))
                        {
                            escapeCode = (span[i + 2] - '0') * 10 + (span[i + 3] - '0');
                            if (startIndex != -1)
                            {
                                _onParseWrite(message, startIndex, length, background, foreground);
                                startIndex = -1;
                                length = 0;
                            }
                            if (TryGetForegroundColor(escapeCode, isBright, out color))
                            {
                                foreground = color;
                                isBright = false;
                            }
                            else if (TryGetBackgroundColor(escapeCode, out color))
                            {
                                background = color;
                            }
                            i += 4;
                            continue;
                        }
                    }
                }
                if (startIndex == -1)
                {
                    startIndex = i;
                }
                int nextEscapeIndex = -1;
                if (i < message.Length - 1)
                {
                    nextEscapeIndex = message.IndexOf(EscapeChar, i + 1);
                }
                if (nextEscapeIndex < 0)
                {
                    length = message.Length - startIndex;
                    break;
                }
                length = nextEscapeIndex - startIndex;
                i = nextEscapeIndex - 1;
            }
            if (startIndex != -1)
            {
                _onParseWrite(message, startIndex, length, background, foreground);
            }
        }

        internal static string GetForegroundColorEscapeCode(ConsoleColor color)
        {
            return color switch
            {
                ConsoleColor.Black => "\e[30m",
                ConsoleColor.DarkRed => "\e[31m",
                ConsoleColor.DarkGreen => "\e[32m",
                ConsoleColor.DarkYellow => "\e[33m",
                ConsoleColor.DarkBlue => "\e[34m",
                ConsoleColor.DarkMagenta => "\e[35m",
                ConsoleColor.DarkCyan => "\e[36m",
                ConsoleColor.Gray => "\e[37m",
                ConsoleColor.Red => "\e[1m\e[31m",
                ConsoleColor.Green => "\e[1m\e[32m",
                ConsoleColor.Yellow => "\e[1m\e[33m",
                ConsoleColor.Blue => "\e[1m\e[34m",
                ConsoleColor.Magenta => "\e[1m\e[35m",
                ConsoleColor.Cyan => "\e[1m\e[36m",
                ConsoleColor.White => "\e[1m\e[37m",
                _ => DefaultForegroundColor // default foreground color
            };
        }

        internal static string GetBackgroundColorEscapeCode(ConsoleColor color)
        {
            return color switch
            {
                ConsoleColor.Black => "\e[40m",
                ConsoleColor.DarkRed => "\e[41m",
                ConsoleColor.DarkGreen => "\e[42m",
                ConsoleColor.DarkYellow => "\e[43m",
                ConsoleColor.DarkBlue => "\e[44m",
                ConsoleColor.DarkMagenta => "\e[45m",
                ConsoleColor.DarkCyan => "\e[46m",
                ConsoleColor.Gray => "\e[47m",
                _ => DefaultBackgroundColor // Use default background color
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsDigit(char c) => (uint)(c - '0') <= '9' - '0';

        private static bool TryGetForegroundColor(int number, bool isBright, out ConsoleColor? color)
        {
            color = number switch
            {
                30 => ConsoleColor.Black,
                31 => isBright ? ConsoleColor.Red : ConsoleColor.DarkRed,
                32 => isBright ? ConsoleColor.Green : ConsoleColor.DarkGreen,
                33 => isBright ? ConsoleColor.Yellow : ConsoleColor.DarkYellow,
                34 => isBright ? ConsoleColor.Blue : ConsoleColor.DarkBlue,
                35 => isBright ? ConsoleColor.Magenta : ConsoleColor.DarkMagenta,
                36 => isBright ? ConsoleColor.Cyan : ConsoleColor.DarkCyan,
                37 => isBright ? ConsoleColor.White : ConsoleColor.Gray,
                _ => null
            };
            return color != null || number == 39;
        }

        private static bool TryGetBackgroundColor(int number, out ConsoleColor? color)
        {
            color = number switch
            {
                40 => ConsoleColor.Black,
                41 => ConsoleColor.DarkRed,
                42 => ConsoleColor.DarkGreen,
                43 => ConsoleColor.DarkYellow,
                44 => ConsoleColor.DarkBlue,
                45 => ConsoleColor.DarkMagenta,
                46 => ConsoleColor.DarkCyan,
                47 => ConsoleColor.Gray,
                _ => null
            };
            return color != null || number == 49;
        }
    }
}
