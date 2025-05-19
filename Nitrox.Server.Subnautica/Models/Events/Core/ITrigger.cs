using System.Linq;
using System.Threading;
using Microsoft.Extensions.Hosting;

namespace Nitrox.Server.Subnautica.Models.Events.Core;

/// <summary>
///     An event trigger which calls all <see cref="TListen" />[] via its <see cref="Trigger" /> method.
/// </summary>
internal interface ITrigger<out TListen, in TContext> : IHostedService where TListen : IListen<TListen, TContext>
{
    public ILogger Logger { get; }
    public TListen[] EventListeners { get; }

    /// <summary>
    ///     Notify all listeners that this event has triggered, with optional context.
    /// </summary>
    ValueTask Trigger(TContext context = default);

    Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        Logger.ZLogTrace($"{EventListeners.Length:@Count} {typeof(TListen).Name:@TypeName} registered - {string.Join(", ", EventListeners.Select(e => e.GetType().Name).Order())}");
        return Task.CompletedTask;
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
