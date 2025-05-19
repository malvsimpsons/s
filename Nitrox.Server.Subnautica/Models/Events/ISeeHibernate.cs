using Nitrox.Server.Subnautica.Models.Events.Core;

namespace Nitrox.Server.Subnautica.Models.Events;

internal interface ISeeHibernate : IParallelListen<ISeeHibernate, object>
{
    ValueTask Hibernate();
    ValueTask IListen<ISeeHibernate, object>.HandleEvent(object context) => Hibernate();
}
