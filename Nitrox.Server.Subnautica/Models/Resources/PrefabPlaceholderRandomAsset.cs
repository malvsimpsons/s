using System.Collections.Generic;
using NitroxModel.DataStructures.Unity;

namespace Nitrox.Server.Subnautica.Models.Resources;

public record struct PrefabPlaceholderRandomAsset(List<string> ClassIds, NitroxTransform Transform = null, string ClassId = null) : IPrefabAsset
{
    public NitroxTransform Transform { get; set; } = Transform;
}
