using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Nitrox.Server.Subnautica.Database.Core.Annotations.Core;

internal interface ITypeNamedAnnotation : IAnnotation
{
    static abstract string TypedName { get; }
}
