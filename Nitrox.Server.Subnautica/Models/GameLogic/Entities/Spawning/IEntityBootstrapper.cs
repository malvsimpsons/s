using Nitrox.Server.Subnautica.Models.Helper;
using NitroxModel.DataStructures.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.GameLogic.Entities.Spawning;

public interface IEntityBootstrapper
{
    public void Prepare(ref WorldEntity spawnedEntity, DeterministicGenerator generator);
}

