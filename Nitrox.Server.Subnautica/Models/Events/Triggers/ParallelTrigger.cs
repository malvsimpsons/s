using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Nitrox.Server.Subnautica.Models.Events.Core;

namespace Nitrox.Server.Subnautica.Models.Events.Triggers;

internal sealed class ParallelTrigger<TListen, TContext>(IServiceProvider provider, ILogger<ParallelTrigger<TListen, TContext>> logger) : ITrigger<TListen, TContext> where TListen : IParallelListen<TListen, TContext>
{
    private readonly IServiceProvider provider = provider;
    private TListen[] eventListeners;

    public ILogger Logger { get; } = logger;
    public TListen[] EventListeners => eventListeners ??= provider.GetRequiredService<IEnumerable<TListen>>().ToArray();

    public async ValueTask Trigger(TContext context = default)
    {
        logger.LogEventTriggering(typeof(TListen).Name, EventListeners.Length);
        List<ValueTask> tasks = [];
        foreach (TListen listener in EventListeners)
        {
            tasks.Add(listener.HandleEvent(context));
        }
        foreach (ValueTask task in tasks)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                Logger.ZLogError(ex, $"");
            }
        }
    }
}
