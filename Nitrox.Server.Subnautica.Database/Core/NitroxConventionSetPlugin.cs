using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Nitrox.Server.Subnautica.Database.Core.Conventions;

namespace Nitrox.Server.Subnautica.Database.Core;

internal sealed class NitroxConventionSetPlugin : IConventionSetPlugin
{
    public ConventionSet ModifyConventions(ConventionSet conventionSet)
    {
        conventionSet.PropertyAddedConventions.Add(new DefaultValueAttributeConvention(null));
        conventionSet.EntityTypeAddedConventions.Add(new TruncateOnStartupAttributeConvention(null));
        return conventionSet;
    }
}
