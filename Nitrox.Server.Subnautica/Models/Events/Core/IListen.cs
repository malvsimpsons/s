namespace Nitrox.Server.Subnautica.Models.Events.Core;

internal interface IListen<TSelf, in TContext> where TSelf : IListen<TSelf, TContext>
{
    ValueTask HandleEvent(TContext context);
}
