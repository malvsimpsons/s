using System;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using NitroxModel.DataStructures.Unity;

namespace Nitrox.Server.Subnautica.Models.Resources;

[Serializable]
/// <summary>
/// Some PrefabPlaceholders spawn GameObjects that are always there (decor, environment ...)
/// And some others spawn a GameObject with an EntitySlot in which case this field is not null.
/// </summary>
public record struct PrefabPlaceholderAsset(string ClassId, NitroxEntitySlot? EntitySlot = null, NitroxTransform Transform = null) : IPrefabAsset;
