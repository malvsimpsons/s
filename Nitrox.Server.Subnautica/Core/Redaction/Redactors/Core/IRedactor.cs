using System;

namespace Nitrox.Server.Subnautica.Core.Redaction.Redactors.Core;

internal interface IRedactor
{
    string[] RedactableKeys { get; }
    RedactResult Redact(ReadOnlySpan<char> key, ReadOnlySpan<char> value);
}
