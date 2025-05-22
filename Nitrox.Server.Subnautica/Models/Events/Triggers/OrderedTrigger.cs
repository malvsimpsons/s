using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Nitrox.Server.Subnautica.Models.Events.Core;

namespace Nitrox.Server.Subnautica.Models.Events.Triggers;

internal sealed class OrderedTrigger<TListen, TContext>(IServiceProvider provider, ILogger<OrderedTrigger<TListen, TContext>> logger) : ITrigger<TListen, TContext> where TListen : IOrderedListen<TListen, TContext>
{
    private readonly IServiceProvider provider = provider;
    private TListen[] eventListeners;

    public ILogger Logger { get; } = logger;
    public TListen[] EventListeners => eventListeners ??= provider.GetRequiredService<IEnumerable<TListen>>().OrderByDescending(e => e.EventPriority).ToArray();

    public async ValueTask Trigger(TContext context = default)
    {
        foreach (TListen listener in EventListeners)
        {
            try
            {
                await listener.HandleEvent(context);
            }
            catch (Exception ex)
            {
                Logger.ZLogError(ex, $"");
            }
        }
    }
}
