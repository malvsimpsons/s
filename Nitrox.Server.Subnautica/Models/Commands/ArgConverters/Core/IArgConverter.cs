namespace Nitrox.Server.Subnautica.Models.Commands.ArgConverters.Core;

internal interface IArgConverter
{
    ConvertResult Convert(object from);
}

/// <summary>
///     Converts an object of type <see cref="TFrom" /> to <see cref="TTo" />.
/// </summary>
internal interface IArgConverter<in TFrom, TTo> : IArgConverter
{
    ConvertResult Convert(TFrom from);

    ConvertResult IArgConverter.Convert(object from) => Convert(from is TFrom tFrom ? tFrom : default);
}
