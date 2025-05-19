using Nitrox.Server.Subnautica.Models.Events.Triggers;

namespace Nitrox.Server.Subnautica.Models.Events.Core;

/// <summary>
///     Implementor listens for an event <see cref="TSelf" /> which will be triggered by
///     <see cref="ParallelTrigger{TListen,TContext}" />.
/// </summary>
internal interface IParallelListen<TSelf, in T> : IListen<TSelf, T> where TSelf : IListen<TSelf, T>;
