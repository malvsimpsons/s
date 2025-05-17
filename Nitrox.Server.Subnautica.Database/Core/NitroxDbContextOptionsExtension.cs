using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Nitrox.Server.Subnautica.Database.Core;

internal sealed class NitroxDbContextOptionsExtension : IDbContextOptionsExtension
{
    public DbContextOptionsExtensionInfo Info => new CustomDbContextOptionsExtensionInfo(this);

    public void ApplyServices(IServiceCollection services) => services.AddSingleton<IConventionSetPlugin, NitroxConventionSetPlugin>();

    public void Validate(IDbContextOptions options) { }

    private class CustomDbContextOptionsExtensionInfo(IDbContextOptionsExtension extension) : DbContextOptionsExtensionInfo(extension)
    {
        public override bool IsDatabaseProvider => false;
        public override string LogFragment => "";
        public override int GetServiceProviderHashCode() => 0;
        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other) => true;

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo) { }
    }
}
