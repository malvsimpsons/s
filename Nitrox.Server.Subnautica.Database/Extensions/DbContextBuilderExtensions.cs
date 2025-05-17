using Microsoft.EntityFrameworkCore.Infrastructure;
using Nitrox.Server.Subnautica.Database.Core;
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
        options.AddInterceptors(new AddSqliteFunctionsInterceptor());
    }
}
