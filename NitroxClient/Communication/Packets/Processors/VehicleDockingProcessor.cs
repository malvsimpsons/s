using System.Collections;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class VehicleDockingProcessor : ClientPacketProcessor<VehicleDocking>
    {
        private readonly IPacketSender packetSender;
        private readonly Vehicles vehicles;

        public VehicleDockingProcessor(IPacketSender packetSender, Vehicles vehicles)
        {
            this.packetSender = packetSender;
            this.vehicles = vehicles;
        }

        public override void Process(VehicleDocking packet)
        {
            GameObject vehicleGo = NitroxEntity.RequireObjectFrom(packet.VehicleId);
            GameObject vehicleDockingBayGo = NitroxEntity.RequireObjectFrom(packet.DockId);
#if SUBNAUTICA
            Vehicle vehicle = vehicleGo.RequireComponent<Vehicle>();
            VehicleDockingBay vehicleDockingBay = vehicleDockingBayGo.RequireComponent<VehicleDockingBay>();

            using (PacketSuppressor<VehicleDocking>.Suppress())
            {
                Log.Debug($"Set vehicle docked for {vehicleDockingBay.gameObject.name}");
                vehicle.GetComponent<MultiplayerVehicleControl>().SetPositionVelocityRotation(vehicle.transform.position, Vector3.zero, vehicle.transform.rotation, Vector3.zero);
                vehicle.GetComponent<MultiplayerVehicleControl>().Exit();
            }
            vehicle.StartCoroutine(DelayAnimationAndDisablePiloting(vehicle, vehicleDockingBay, packet.VehicleId, packet.PlayerId));
#elif BELOWZERO
            Dockable dockable = vehicleGo.RequireComponent<Dockable>();
            VehicleDockingBay vehicleDockingBay = vehicleDockingBayGo.RequireComponent<VehicleDockingBay>();

            using (PacketSuppressor<VehicleDocking>.Suppress())
            {
                Log.Debug($"Set docked for {vehicleDockingBay.gameObject.name}");
                dockable.GetComponent<MultiplayerVehicleControl>().SetPositionVelocityRotation(dockable.transform.position, Vector3.zero, dockable.transform.rotation, Vector3.zero);
                dockable.GetComponent<MultiplayerVehicleControl>().Exit();
            }
            dockable.StartCoroutine(DelayAnimationAndDisablePiloting(dockable, vehicleDockingBay, packet.VehicleId, packet.PlayerId));
#endif
        }

#if SUBNAUTICA
        IEnumerator DelayAnimationAndDisablePiloting(Vehicle vehicle, VehicleDockingBay vehicleDockingBay, NitroxId vehicleId, ushort playerId)
#elif BELOWZERO
        IEnumerator DelayAnimationAndDisablePiloting(Dockable vehicle, VehicleDockingBay vehicleDockingBay, NitroxId vehicleId, ushort playerId)
#endif
        {
            yield return Yielders.WaitFor1Second;
            // DockVehicle sets the rigid body kinematic of the vehicle to true, we don't want that behaviour
            // Therefore disable kinematic (again) to remove the bouncing behavior
#if SUBNAUTICA
            vehicleDockingBay.DockVehicle(vehicle);
            vehicle.useRigidbody.isKinematic = false;
#elif BELOWZERO
            vehicleDockingBay.Dock(vehicle);
#endif
            yield return Yielders.WaitFor2Seconds;
            vehicles.SetOnPilotMode(vehicleId, playerId, false);
            if (!vehicle.docked)
            {
                Log.Error($"Vehicle {vehicleId} not docked after docking process");
            }
        }
    }
}
