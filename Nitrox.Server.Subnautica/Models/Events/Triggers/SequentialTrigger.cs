using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Nitrox.Server.Subnautica.Models.Events.Core;

namespace Nitrox.Server.Subnautica.Models.Events.Triggers;

internal sealed class SequentialTrigger<TListen, TContext>(IServiceProvider provider, ILogger<SequentialTrigger<TListen, TContext>> logger) : ITrigger<TListen, TContext> where TListen : IListen<TListen, TContext>
{
    private readonly IServiceProvider provider = provider;
    private TListen[] eventListeners;

    public ILogger Logger { get; } = logger;
    public TListen[] EventListeners => eventListeners ??= provider.GetRequiredService<IEnumerable<TListen>>().ToArray();

    public async ValueTask Trigger(TContext context = default)
    {
        logger.LogEventTriggering(typeof(TListen).Name, EventListeners.Length);
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
