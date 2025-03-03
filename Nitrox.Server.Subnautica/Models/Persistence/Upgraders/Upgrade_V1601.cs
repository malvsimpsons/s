using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Nitrox.Server.Subnautica.Models.Persistence.Core;
using NitroxModel.DataStructures;

namespace Nitrox.Server.Subnautica.Models.Persistence.Upgraders;

internal class Upgrade_V1601 : IStateUpgrader
{
    public async Task UpgradeAsync()
    {
        await IStateUpgrader.UpgradeJsonFileAsync("WorldData", UpgradeWorldData);
    }

    private void UpgradeWorldData(JsonNode data)
    {
        List<string> modules = new();
        foreach (JsonNode moduleEntry in data["InventoryData"]["Modules"].AsArray())
        {
            JsonNode itemId = moduleEntry["ItemId"];
            if (itemId == null)
            {
                continue;
            }
            if (modules.Contains(itemId.ToString()))
            {
                itemId = new NitroxId().ToString();
                // this line is enough to modify the original data
                moduleEntry["ItemId"] = itemId;
            }
            modules.Add(itemId.ToString());
        }
    }
}
