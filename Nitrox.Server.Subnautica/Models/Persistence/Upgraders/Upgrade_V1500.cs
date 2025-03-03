using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Nitrox.Server.Subnautica.Models.Persistence.Core;

namespace Nitrox.Server.Subnautica.Models.Persistence.Upgraders;

internal sealed class Upgrade_V1500 : IStateUpgrader
{
    public async Task UpgradeAsync()
    {
        await IStateUpgrader.UpgradeJsonFileAsync("WorldData", UpgradeWorldData);

        Log.Warn("Plants will still be counted as normal items with no growth progression. Re adding them to a container should fix this.");
        Log.Warn("The precursor incubator may be unpowered and hatching progress will be reset");
    }

    private void UpgradeWorldData(JsonNode data)
    {
        data["GameData"]["StoryTiming"] = data["StoryTimingData"];
        data.AsObject().Remove("StoryTimingData");
        data["Seed"] = "TCCBIBZXAB"; //Default seed so life pod should stay the same
        data["InventoryData"]["Modules"] = new JsonArray();
    }
}
