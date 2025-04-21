using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxModel_Subnautica.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class CyclopsFireCreatedProcessor : IClientPacketProcessor<CyclopsFireCreated>
    {
        private readonly IPacketSender packetSender;
        private readonly Fires fires;

        public CyclopsFireCreatedProcessor(IPacketSender packetSender, Fires fires)
        {
            this.packetSender = packetSender;
            this.fires = fires;
        }

        public Task Process(IPacketProcessContext context, CyclopsFireCreated packet)
        {
            fires.Create(packet.FireCreatedData);

            return Task.CompletedTask;
        }
    }
}
