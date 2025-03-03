using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Nitrox.Server.Subnautica.Models.Configuration;
using Nitrox.Server.Subnautica.Models.Persistence.Core;
using NitroxModel.Helper;

namespace Nitrox.Server.Subnautica.Models.Persistence.Upgraders;

public sealed class ServerStateUpgrader
{
    private readonly IOptions<ServerStartOptions> optionsProvider;
    public static readonly Version MinimumSaveVersion = new(1, 8, 0, 0);

    public readonly SortedDictionary<Version, IStateUpgrader> Upgraders = new()
    {
        [new Version(1, 6, 0, 1)] = new Upgrade_V1601(),
        [new Version(1, 6, 0, 0)] = new Upgrade_V1600(),
        [new Version(1, 5, 0, 0)] = new Upgrade_V1500(),
    };

    public ServerStateUpgrader(JsonSerializerOptions serializerOptions, IOptions<ServerStartOptions> optionsProvider)
    {
        this.optionsProvider = optionsProvider;
        IStateUpgrader.SerializerOptions = serializerOptions;
        IStateUpgrader.StateRootPath = Path.Combine(KeyValueStore.Instance.GetServerSavesPath(), this.optionsProvider.Value.SaveName);
    }
}
