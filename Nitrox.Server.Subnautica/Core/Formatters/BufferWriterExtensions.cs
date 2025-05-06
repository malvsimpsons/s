using System;
using System.Buffers;
using System.Text;

namespace Nitrox.Server.Subnautica.Core.Formatters;

internal static class BufferWriterExtensions
{
    private static byte[] environmentNewLineUtf8;

    private static ReadOnlySpan<byte> EnvironmentNewLineUtf8
    {
        get
        {
            if (environmentNewLineUtf8 is null or [])
            {
                environmentNewLineUtf8 = Encoding.UTF8.GetBytes(Environment.NewLine);
            }
            return environmentNewLineUtf8;
        }
    }

    public static void WriteLine(this IBufferWriter<byte> writer) => writer.Write(EnvironmentNewLineUtf8);

    public static void Write(this IBufferWriter<byte> writer, string value)
    {
        writer.Write(Encoding.UTF8.GetBytes(value));
    }
}
