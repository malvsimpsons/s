using Nitrox.Server.Subnautica.Models.Helper;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;

namespace Nitrox.Server.Subnautica.Models.GameLogic.Entities.Spawning;

public class StayAtLeashPositionBootstrapper : IEntityBootstrapper
{
    public void Prepare(ref WorldEntity spawnedEntity, DeterministicGenerator generator)
    {
        spawnedEntity.Metadata = new StayAtLeashPositionMetadata(spawnedEntity.Transform.Position);
    }
}
