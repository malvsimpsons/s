using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Nitrox.Server.Subnautica.Database.Core.Annotations;
using Nitrox.Server.Subnautica.Database.Core.Attributes;

namespace Nitrox.Server.Subnautica.Database.Core.Conventions;

internal sealed class TruncateOnStartupAttributeConvention(ProviderConventionSetBuilderDependencies dependencies) : TypeAttributeConventionBase<TruncateOnStartupAttribute>(dependencies)
{
    protected override void ProcessEntityTypeAdded(IConventionEntityTypeBuilder entityTypeBuilder, TruncateOnStartupAttribute attribute, IConventionContext<IConventionEntityTypeBuilder> context)
    {
        entityTypeBuilder.HasAnnotation(TruncateOnStartupAnnotation.TypedName, true, true);
    }
}
