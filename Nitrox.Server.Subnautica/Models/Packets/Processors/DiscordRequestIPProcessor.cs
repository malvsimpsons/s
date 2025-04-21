using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Nitrox.Server.Subnautica.Models.Configuration;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxModel.Serialization;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

public class DiscordRequestIPProcessor(IOptions<SubnauticaServerOptions> optionsProvider) : AuthenticatedPacketProcessor<DiscordRequestIP>
{
    private readonly IOptions<SubnauticaServerOptions> optionsProvider = optionsProvider;

    private string ipPort;

    public override void Process(DiscordRequestIP packet, NitroxServer.Player player)
    {
        if (string.IsNullOrEmpty(ipPort))
        {
            Task.Run(() => ProcessPacketAsync(packet, player));
            return;
        }

        packet.IpPort = ipPort;
        player.SendPacket(packet);
    }

    private async Task ProcessPacketAsync(DiscordRequestIP packet, NitroxServer.Player player)
    {
        string result = await GetIpAsync();
        if (result == "")
        {
            Log.Error("Couldn't get external Ip for discord request.");
            return;
        }

        packet.IpPort = ipPort = $"{result}:{optionsProvider.Value.ServerPort}";
        player.SendPacket(packet);
    }

    /// <summary>
    /// Get the WAN IP address or the Hamachi IP address if the WAN IP address is not available.
    /// </summary>
    /// <returns>Found IP or blank string if none found</returns>
    private static async Task<string> GetIpAsync()
    {
        Task<IPAddress> wanIp = NetHelper.GetWanIpAsync();
        Task<IPAddress> hamachiIp = Task.Run(NetHelper.GetHamachiIp);
        if (await wanIp != null)
        {
            return wanIp.Result.ToString();
        }

        if (await hamachiIp != null)
        {
            return hamachiIp.Result.ToString();
        }
        return "";
    }
}
