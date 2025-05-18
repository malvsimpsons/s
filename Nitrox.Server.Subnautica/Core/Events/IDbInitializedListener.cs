namespace Nitrox.Server.Subnautica.Core.Events;

internal interface IDbInitializedListener
{
    Task DatabaseInitialized();
}
