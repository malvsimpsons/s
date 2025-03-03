using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Nitrox.Server.Subnautica.Models.Persistence.Core;

namespace Nitrox.Server.Subnautica.Models.Persistence.Upgraders;

internal class Upgrade_V1600 : IStateUpgrader
{
    public async Task UpgradeAsync() => await IStateUpgrader.UpgradeJsonFileAsync("WorldData", UpgradeWorldData);

    private void UpgradeWorldData(JsonNode data)
    {
        List<string> cleanUnlockedTechTypes = data["GameData"]["PDAState"]["UnlockedTechTypes"].AsArray().GetValues<string>().Distinct().ToList();
        List<string> cleanKnownTechTypes = data["GameData"]["PDAState"]["KnownTechTypes"].AsArray().GetValues<string>().Distinct().ToList();
        List<string> cleanEncyclopediaEntries = data["GameData"]["PDAState"]["EncyclopediaEntries"].AsArray().GetValues<string>().Distinct().ToList();
        data["GameData"]["PDAState"]["UnlockedTechTypes"] = new JsonArray([.. cleanUnlockedTechTypes]);
        data["GameData"]["PDAState"]["KnownTechTypes"] = new JsonArray([.. cleanKnownTechTypes]);
        data["GameData"]["PDAState"]["EncyclopediaEntries"] = new JsonArray([.. cleanEncyclopediaEntries]);

        List<JsonNode> cleanPdaLog = [];
        JsonArray pdaLog = data["GameData"]["PDAState"]["PdaLog"].AsArray();
        foreach (JsonNode pdaLogEntry in pdaLog)
        {
            string Key = pdaLogEntry["Key"].ToString();
            if (cleanPdaLog.All(entry => entry["Key"].ToString() != Key))
            {
                cleanPdaLog.Add(pdaLogEntry);
            }
        }
        data["GameData"]["PDAState"]["PdaLog"] = new JsonArray([.. cleanPdaLog]);

        data.AsObject().Remove("ServerStartTime");
    }
}
