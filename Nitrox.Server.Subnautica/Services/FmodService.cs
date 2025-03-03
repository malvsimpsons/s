using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NitroxModel.GameLogic.FMOD;

namespace Nitrox.Server.Subnautica.Services;

internal sealed class FmodService(GameInfo gameInfo) : IHostedService
{
    private readonly GameInfo gameInfo = gameInfo;
    private FmodWhitelist whitelist;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        whitelist = FmodWhitelist.Load(gameInfo);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
