using Microsoft.EntityFrameworkCore.Metadata;

namespace Nitrox.Server.Subnautica.Database.Extensions;

internal static class EntityTypeExtensions
{
    public static bool HasAnnotationWithValue<T>(this IEntityType entityType, string annotationName, T value)
    {
        if (entityType.FindAnnotation(annotationName) is not { } annotation)
        {
            return false;
        }
        return annotation.Value is T val && val.Equals(value);
    }
}
