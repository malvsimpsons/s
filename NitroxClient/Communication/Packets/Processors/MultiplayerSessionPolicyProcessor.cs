using NitroxClient.Communication.Abstract;
using NitroxModel.Networking.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class MultiplayerSessionPolicyProcessor : IClientPacketProcessor<SessionPolicy>
    {
        private readonly IMultiplayerSession multiplayerSession;

        public MultiplayerSessionPolicyProcessor(IMultiplayerSession multiplayerSession)
        {
            this.multiplayerSession = multiplayerSession;
        }

        public Task Process(IPacketProcessContext context, SessionPolicy packet)
        {
            Log.Info($"Processing session policy {packet}");
            multiplayerSession.ProcessSessionPolicy(packet);

            return Task.CompletedTask;
        }
    }
}
