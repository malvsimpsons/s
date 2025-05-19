using Nitrox.Server.Subnautica.Models.Helper;
using NitroxModel.DataStructures.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.GameLogic.Entities.Spawning;

public interface IEntityBootstrapperManager
{
    public void PrepareEntityIfRequired(ref WorldEntity spawnedEntity, DeterministicGenerator generator);
}
