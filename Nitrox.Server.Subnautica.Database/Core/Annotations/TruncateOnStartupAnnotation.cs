using Nitrox.Server.Subnautica.Database.Core.Annotations.Core;

namespace Nitrox.Server.Subnautica.Database.Core.Annotations;

internal sealed class TruncateOnStartupAnnotation : ITypeNamedAnnotation
{
    public string Name => TypedName;
    public object Value { get; }
    public static string TypedName => "TruncateOnStartup";
}
