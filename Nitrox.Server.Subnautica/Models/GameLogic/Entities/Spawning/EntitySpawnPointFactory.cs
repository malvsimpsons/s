using System.Collections.Generic;
using Nitrox.Server.Subnautica.Models.UnityStubs;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Unity;

namespace Nitrox.Server.Subnautica.Models.GameLogic.Entities.Spawning;

public abstract class EntitySpawnPointFactory
{
    public abstract List<EntitySpawnPoint> From(AbsoluteEntityCell absoluteEntityCell, NitroxTransform transform, GameObject gameObject);
}
