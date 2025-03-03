using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Nitrox.Server.Subnautica.Core.Configuration.Providers;

namespace Nitrox.Server.Subnautica.Extensions;

internal static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddNitroxConfigFile(this IConfigurationBuilder configurationBuilder, string fullConfigFilePath, string configSectionPath = "", bool optional = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullConfigFilePath);
        string dirPath = Path.GetDirectoryName(fullConfigFilePath);
        Directory.CreateDirectory(dirPath);
        return configurationBuilder.Add(new NitroxConfigurationSource(Path.GetFileName(dirPath), configSectionPath, optional, new PhysicalFileProvider(dirPath)));
    }
}
