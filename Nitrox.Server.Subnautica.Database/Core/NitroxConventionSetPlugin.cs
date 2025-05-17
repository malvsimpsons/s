using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Nitrox.Server.Subnautica.Database.Core;

internal sealed class NitroxConventionSetPlugin : IConventionSetPlugin
{
    public ConventionSet ModifyConventions(ConventionSet conventionSet)
    {
        conventionSet.PropertyAddedConventions.Add(new DefaultValueAttributeConvention(null));
        return conventionSet;
    }
}
