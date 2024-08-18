#if BELOWZERO
using System.Linq;
#endif
using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
#if BELOWZERO
using NitroxModel_Subnautica.DataStructures;
#endif

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class ExosuitMetadataExtractor : EntityMetadataExtractor<Exosuit, ExosuitMetadata>
{
    public override ExosuitMetadata Extract(Exosuit exosuit)
    {
        LiveMixin liveMixin = exosuit.liveMixin;
#if SUBNAUTICA
        SubName subName = exosuit.subName;

        return new(liveMixin.health, SubNameInputMetadataExtractor.GetName(subName), SubNameInputMetadataExtractor.GetColors(subName));
#elif BELOWZERO
        ColorNameControl colorNameControl = exosuit.colorNameControl;

        return new(liveMixin.health, colorNameControl.savedName, colorNameControl.savedColors.Select(color => color.ToDto()).ToArray());
#endif

    }
}
