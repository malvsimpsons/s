using NitroxClient.Communication.Abstract;
using NitroxModel.Networking.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class PDALogEntryAddProcessor : IClientPacketProcessor<PDALogEntryAdd>
    {
        private readonly IPacketSender packetSender;

        public PDALogEntryAddProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public Task Process(IPacketProcessContext context, PDALogEntryAdd packet)
        {
            using (PacketSuppressor<PDALogEntryAdd>.Suppress())
            {
                PDALog.Add(packet.Key);
            }

            return Task.CompletedTask;
        }
    }
}
