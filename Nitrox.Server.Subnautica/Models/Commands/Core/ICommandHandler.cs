namespace Nitrox.Server.Subnautica.Models.Commands.Core;

public interface ICommandHandlerBase;

public interface ICommandHandler : ICommandHandlerBase
{
    void Execute(ICommandContext context);
}

public interface ICommandHandler<in TArg1> : ICommandHandlerBase
{
    void Execute(ICommandContext context, TArg1 arg);
}

public interface ICommandHandler<in TArg1, in TArg2> : ICommandHandlerBase
{
    void Execute(ICommandContext context, TArg1 arg1, TArg2 arg2);
}

public interface ICommandHandler<in TArg1, in TArg2, in TArg3> : ICommandHandlerBase
{
    void Execute(ICommandContext context, TArg1 arg1, TArg2 arg2, TArg3 arg3);
}

public interface ICommandHandler<in TArg1, in TArg2, in TArg3, in TArg4> : ICommandHandlerBase
{
    void Execute(ICommandContext context, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4);
}
