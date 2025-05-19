namespace Nitrox.Server.Subnautica.Models.Events.Core;

internal interface IParallelListen<TSelf, in T> : IListen<TSelf, T> where TSelf : IListen<TSelf, T>;
