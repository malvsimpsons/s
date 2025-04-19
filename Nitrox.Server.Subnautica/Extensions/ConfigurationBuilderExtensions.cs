using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Nitrox.Server.Subnautica.Core.Configuration.Providers;
using NitroxModel.Serialization;

namespace Nitrox.Server.Subnautica.Extensions;

internal static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddNitroxConfigFile<TOptions>(this IConfigurationBuilder configurationBuilder, string filePath, string configSectionPath = "", bool optional = true) where TOptions : class, new()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        string dirPath = Path.GetDirectoryName(filePath);
        Directory.CreateDirectory(dirPath ?? throw new ArgumentException(nameof(filePath)));
        if (!File.Exists(filePath))
        {
            NitroxConfig.CreateFile<TOptions>(filePath);
        }

        // Link the config to a relative path within the working directory so that IOptionsMonitor<T> works. See https://github.com/dotnet/runtime/issues/114833
        try
        {
            string targetConfigLink = Path.GetFileName(filePath);
            FileInfo configFile = new(targetConfigLink);
            if (configFile.Exists && configFile.LinkTarget != null)
            {
                configFile.Delete();
            }
            File.CreateSymbolicLink(targetConfigLink, filePath);
            // Fix targets to point to symbolic link instead.
            dirPath = AppContext.BaseDirectory;
            filePath = targetConfigLink;
        }
        catch (IOException)
        {
            // ignored - config change detection isn't critical for server.
        }

        return configurationBuilder.Add(new NitroxConfigurationSource(filePath, configSectionPath, optional, new PhysicalFileProvider(dirPath)
        {
            UsePollingFileWatcher = true,
            UseActivePolling = true
        }));
    }
}
