using System.ComponentModel;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Nitrox.Server.Subnautica.Database.Core.Conventions;

internal sealed class DefaultValueAttributeConvention(ProviderConventionSetBuilderDependencies dependencies) : PropertyAttributeConventionBase<DefaultValueAttribute>(dependencies)
{
    protected override void ProcessPropertyAdded(IConventionPropertyBuilder propertyBuilder, DefaultValueAttribute attribute, MemberInfo clrMember, IConventionContext context)
    {
        if (attribute.Value is string defaultSql)
        {
            propertyBuilder.HasDefaultValueSql(defaultSql, true);
        }
    }
}
