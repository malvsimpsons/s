using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Nitrox.Server.Subnautica.Models.Persistence.Core;
using Nitrox.Server.Subnautica.Models.Resources.Helper;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Saves and restores serializable <see cref="IStateManager.ObjectState"/>s of the server.
/// </summary>
internal sealed class PersistenceService(IEnumerable<IStateManager> states, JsonSerializerOptions serializerOptions, IOptions<Models.Configuration.ServerStartOptions> optionsProvider, ILogger<PersistenceService> logger)
    : IHostedService
{
    private readonly ILogger<PersistenceService> logger = logger;
    private readonly IOptions<Models.Configuration.ServerStartOptions> optionsProvider = optionsProvider;
    private readonly JsonSerializerOptions serializerOptions = serializerOptions;
    private readonly IEnumerable<IStateManager> states = states;

    public async Task StartAsync(CancellationToken cancellationToken) => await LoadAsync(cancellationToken);

    public async Task StopAsync(CancellationToken cancellationToken) => await SaveAsync();

    private async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        foreach (IGrouping<string, IStateManager> group in states.GroupBy(state => state.GroupKey))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await using Stream stream = GetReadStreamByKey(group.Key);
            if (stream is not { Length: > 0 })
            {
                // No file: create default state for the whole group.
                foreach (IStateManager state in group)
                {
                    state.SetOrDefault();
                }
                continue;
            }
            JsonNode node = await JsonNode.ParseAsync(stream, cancellationToken: cancellationToken);
            if (node == null)
            {
                continue;
            }

            // Try to deserialize state or create default if missing from file.
            JsonObject rootNode = node.AsObject();
            if (group.Count() == 1)
            {
                IStateManager manager = group.First();
                manager.SetOrDefault(rootNode.Deserialize(manager.StateType));
            }
            else
            {
                HashSet<IStateManager> skippedManagers = [];
                foreach ((string key, JsonNode value) in rootNode)
                {
                    foreach (IStateManager manager in group)
                    {
                        if (manager.SubKey == key)
                        {
                            skippedManagers.Remove(manager);
                            manager.SetOrDefault(value.Deserialize(manager.StateType, serializerOptions));
                        }
                        else
                        {
                            skippedManagers.Add(manager);
                        }
                    }
                }
                foreach (IStateManager manager in skippedManagers)
                {
                    manager.SetOrDefault();
                }
            }
        }
    }

    public async Task SaveAsync()
    {
        foreach (IGrouping<string, IStateManager> group in states.GroupBy(state => state.GroupKey))
        {
            await using Stream stream = File.OpenWrite(GetPathByKey(group.Key));
            if (group.Count() == 1)
            {
                await System.Text.Json.JsonSerializer.SerializeAsync(stream, group.First().ObjectState, serializerOptions);
            }
            else
            {
                Dictionary<string,object> fileObject = group.ToDictionary(s => s.SubKey, s => s.ObjectState);
                await System.Text.Json.JsonSerializer.SerializeAsync(stream, fileObject, serializerOptions);
            }
        }
    }

    private string GetPathByKey(string key) => Path.Combine(optionsProvider.Value.GetServerSavePath(), Path.ChangeExtension(key, ".json") ?? throw new Exception("Failed to set state file extension to .json"));

    private Stream GetReadStreamByKey(string key)
    {
        try
        {
            return File.OpenRead(GetPathByKey(key));
        }
        catch (FileNotFoundException ex)
        {
            logger.LogDebug(ex.Message);
            return null;
        }
    }
}
