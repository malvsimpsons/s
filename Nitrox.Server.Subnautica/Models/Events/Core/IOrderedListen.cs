using Nitrox.Server.Subnautica.Models.Events.Triggers;

namespace Nitrox.Server.Subnautica.Models.Events.Core;

/// <summary>
///     Implementor listens for an event <see cref="TSelf" /> which will be triggered by
///     <see cref="OrderedTrigger{TListen,TContext}" />.
/// </summary>
internal interface IOrderedListen<TSelf, in TContext> : IListen<TSelf, TContext> where TSelf : IListen<TSelf, TContext>
{
    /// <summary>
    ///     Gets the requested priority of the event listener. Higher values are called before lower values.
    /// </summary>
    public int EventPriority => 0;
}
