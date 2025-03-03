using Nitrox.Server.Subnautica.Models.Commands.ArgConverters.Core;

namespace Nitrox.Server.Subnautica.Models.Commands.ArgConverters;

/// <summary>
///     Converts a word to a bool value.
/// </summary>
internal class WordToBoolArgConverter : IArgConverter<string, bool>
{
    public ConvertResult Convert(string from) =>
        from switch
        {
            "on" or "enable" or "1" => ConvertResult.Ok(true),
            "off" or "disable" or "0" => ConvertResult.Ok(false),
            _ => ConvertResult.Fail()
        };
}
