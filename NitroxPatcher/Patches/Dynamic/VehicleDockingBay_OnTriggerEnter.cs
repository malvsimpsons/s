using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class VehicleDockingBay_OnTriggerEnter : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((VehicleDockingBay t) => t.OnTriggerEnter(default(Collider)));
#if SUBNAUTICA
    private static Vehicle prevInterpolatingVehicle;
#elif BELOWZERO
    private static Dockable prevInterpolatingDockable;
#endif

    public static bool Prefix(VehicleDockingBay __instance, Collider other)
    {
        Vehicle vehicle = other.GetComponentInParent<Vehicle>();
#if SUBNAUTICA
        prevInterpolatingVehicle = __instance.interpolatingVehicle;
#elif BELOWZERO
        prevInterpolatingDockable = __instance.interpolatingDockable;
#endif
        Optional<NitroxId> opVehicleId = vehicle.GetId();
        return !vehicle || (opVehicleId.HasValue && Resolve<SimulationOwnership>().HasAnyLockType(opVehicleId.Value));
    }

    public static void Postfix(VehicleDockingBay __instance)
    {
#if SUBNAUTICA
        Vehicle interpolatingVehicle = __instance.interpolatingVehicle;
        // Only send data, when interpolatingVehicle changes to avoid multiple packages send
        if (!interpolatingVehicle || interpolatingVehicle == prevInterpolatingVehicle)
        {
            return;
        }

        if (interpolatingVehicle.TryGetIdOrWarn(out NitroxId id) && Resolve<SimulationOwnership>().HasAnyLockType(id))
        {
            Log.Debug($"Will send vehicle docking for {id}");
            Resolve<Vehicles>().BroadcastVehicleDocking(__instance, interpolatingVehicle);
        }
#elif BELOWZERO
        Dockable interpolatingDockable = __instance.interpolatingDockable;
        // Only send data, when interpolatingDockable changes to avoid multiple packages send
        if (!interpolatingDockable || interpolatingDockable == prevInterpolatingDockable)
        {
            return;
        }

        if (interpolatingDockable.TryGetIdOrWarn(out NitroxId id) && Resolve<SimulationOwnership>().HasAnyLockType(id))
        {
            Log.Debug($"Will send dockable docking for {id}");
            Resolve<Vehicles>().BroadcastVehicleDocking(__instance, interpolatingDockable.vehicle);
        }
#endif
    }
}
