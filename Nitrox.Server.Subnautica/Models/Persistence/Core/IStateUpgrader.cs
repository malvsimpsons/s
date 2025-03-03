using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Nitrox.Server.Subnautica.Models.Persistence.Core;

public interface IStateUpgrader
{
    Task UpgradeAsync();
    static JsonSerializerOptions SerializerOptions { get; internal set; }

    static string StateRootPath { get; internal set; }

    public static async Task UpgradeJsonFileAsync(string fileName, Action<JsonNode> upgrader)
    {
        string path = Path.Combine(StateRootPath, Path.ChangeExtension(fileName, ".json"));
        JsonNode node = JsonNode.Parse(await File.ReadAllTextAsync(path));
        upgrader?.Invoke(node);
        await using Stream stream = File.Open(path, FileMode.Create);
        node?.WriteTo(new Utf8JsonWriter(stream), SerializerOptions);
    }
}
