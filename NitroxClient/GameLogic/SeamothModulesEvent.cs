using NitroxClient.Communication.Abstract;
using NitroxModel.DataStructures;
using Nitrox.Model.Subnautica.DataStructures;
using NitroxModel.Networking.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class SeamothModulesEvent
    {
        private readonly IPacketSender packetSender;

        public SeamothModulesEvent(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void BroadcastElectricalDefense(TechType techType, int slotID, SeaMoth instance)
        {
            if (!instance.TryGetIdOrWarn(out NitroxId id))
            {
                return;
            }

            if (techType == TechType.SeamothElectricalDefense)
            {
                Transform aimingTransform = Player.main.camRoot.GetAimingTransform();
                SeamothModulesAction changed = new SeamothModulesAction(techType.ToDto(), slotID, id, aimingTransform.forward.ToDto(), aimingTransform.rotation.ToDto());
                packetSender.Send(changed);
            }
        }
    }
}
