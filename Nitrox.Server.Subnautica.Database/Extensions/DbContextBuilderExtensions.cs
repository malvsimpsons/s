using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Nitrox.Server.Subnautica.Database.Core;
using Nitrox.Server.Subnautica.Database.Core.Annotations;
using Nitrox.Server.Subnautica.Database.Core.Interceptors;

namespace Nitrox.Server.Subnautica.Database.Extensions;

public static class DbContextBuilderExtensions
{
    public static void UseNitroxExtensions(this DbContextOptionsBuilder options)
    {
        if (options is IDbContextOptionsBuilderInfrastructure infraOptions)
        {
            infraOptions.AddOrUpdateExtension(new NitroxDbContextOptionsExtension());
        }
        options.AddInterceptors(new DisableInternalEntityFrameworkPragmaInterceptor(), new AddSqliteFunctionsInterceptor());
        options.UseAsyncSeeding(async (context, _, cancellationToken) =>
        {
            foreach (IEntityType entityType in context.Model.GetEntityTypes())
            {
                if (entityType.HasAnnotationWithValue(TruncateOnStartupAnnotation.TypedName, true))
                {
#pragma warning disable EF1002
                    await context.Database.ExecuteSqlRawAsync($"""
                                                               DELETE FROM {entityType.GetTableName()}; -- Removes all data from table.
                                                               DELETE FROM SQLITE_SEQUENCE WHERE name='{entityType.GetTableName()}'; -- Clears auto increment sequence (not always used by a table, but good to be on safe side).
                                                               """,
                                                              cancellationToken);
#pragma warning restore EF1002
                }
            }
        });
    }
}
