using Nitrox.Server.Subnautica.Models.Events.Triggers;

namespace Nitrox.Server.Subnautica.Models.Events.Core;

/// <summary>
///     Implementor listens for an event <see cref="TSelf" /> which will be triggered by
///     <see cref="SequentialTrigger{TListen,TContext}" />.
/// </summary>
internal interface IListen<TSelf, in TContext> where TSelf : IListen<TSelf, TContext>
{
    /// <summary>
    ///     Handler which is called when the <see cref="TSelf" /> event triggers via a
    ///     <see cref="ITrigger{TListen,TContext}" />.
    /// </summary>
    ValueTask HandleEvent(TContext context);
}
