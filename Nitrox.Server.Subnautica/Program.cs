using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Nitrox.Server.Subnautica.Core;
using Nitrox.Server.Subnautica.Models.Configuration;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Bases;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities.Spawning;
using Nitrox.Server.Subnautica.Models.Helper;
using Nitrox.Server.Subnautica.Models.Resources;
using Nitrox.Server.Subnautica.Models.Serialization;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Helper;
using NitroxModel.Networking;
using NitroxServer.GameLogic.Entities.Spawning;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Nitrox.Server.Subnautica;

public class Program
{
    private const string APPSETTINGS_DEVELOPMENT_JSON = "server.Development.json";

    private static ServerStartOptions startOptions;
    private static readonly Stopwatch serverStartStopWatch = new();
    private static readonly Lazy<string> newWorldSeed = new(() => StringHelper.GenerateRandomString(10));

    private static async Task Main(string[] args)
    {
        AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolver.Handler;
        AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += AssemblyResolver.Handler;

        await StartupHostAsync(args);
    }

    /// <summary>
    ///     Initialize here so that the JIT can compile the EntryPoint method without having to resolve dependencies
    ///     that require the custom <see cref="AssemblyResolver.Handler" />.
    /// </summary>
    /// <remarks>
    ///     https://stackoverflow.com/a/6089153/1277156
    /// </remarks>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static async Task StartupHostAsync(string[] args)
    {
        serverStartStopWatch.Start();

        // Parse console args into config object for type-safety.
        IConfigurationRoot configuration = new ConfigurationBuilder()
                                           .AddCommandLine(args)
                                           .Build();
        startOptions = new ServerStartOptions();
        configuration.Bind(startOptions);
        startOptions.GameInstallPath ??= NitroxUser.GamePath;

        // TODO: Do not depend on Assembly-Csharp types, only game files. Use proxy/stub types which can map to a Subnautica object.
        AssemblyResolver.GameInstallPath = startOptions.GameInstallPath;

        if (Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") is null)
        {
            const string COMPILE_ENV =
#if DEBUG
                    "Development"
#else
                    "Production"
#endif
                ;
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", COMPILE_ENV);
        }

        await StartServerAsync(args);
    }

    private static async Task StartServerAsync(string[] args)
    {
        // TODO: Don't use NitroxModel.Log in this project.

        // TODO: pass logs to serilog with rolling log files strategy.

        // TODO: Move to separate services
        // if (useLANBroadcast)
        // {
        //     LANBroadcastServer.Start(ct);
        // }

        HostApplicationBuilder builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings
        {
            DisableDefaults = true,
            EnvironmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT"),
            ApplicationName = "Nitrox.Server.Subnautica"
        });
        // Nitrox config can be overriden by development.json or command line args if supplied.
        builder.Configuration.AddNitroxConfigFile<SubnauticaServerOptions>(startOptions.GetServerConfigFilePath(), SubnauticaServerOptions.CONFIG_SECTION_PATH);
        if (builder.Environment.IsDevelopment())
        {
            // Symbolic link the first parent config.json found to the working directory.
            string current = AppContext.BaseDirectory.TrimEnd('/', '\\');
            while ((current = Path.GetDirectoryName(current)) is not null)
            {
                string parentAppSettingsFile = Path.Combine(current, APPSETTINGS_DEVELOPMENT_JSON);
                if (File.Exists(parentAppSettingsFile))
                {
                    FileInfo appSettingsFile = new(Path.Combine(AppContext.BaseDirectory, APPSETTINGS_DEVELOPMENT_JSON));
                    if (appSettingsFile.Exists && appSettingsFile.LinkTarget != null)
                    {
                        appSettingsFile.Delete();
                    }
                    appSettingsFile.CreateAsSymbolicLink(parentAppSettingsFile);
                    break;
                }
            }

            // On Linux, polling is needed to detect file changes.
            builder.Configuration.AddJsonFile(new PhysicalFileProvider(AppContext.BaseDirectory)
            {
                UseActivePolling = OperatingSystem.IsLinux(),
                UsePollingFileWatcher = OperatingSystem.IsLinux()
            }, APPSETTINGS_DEVELOPMENT_JSON, true, true);
        }
        builder.Configuration.AddCommandLine(args);
        builder.Logging
               .SetMinimumLevel(builder.Environment.IsDevelopment() ? LogLevel.Debug : LogLevel.Information)
               .AddFilter("Nitrox.Server.Subnautica", level => level > LogLevel.Trace || (level == LogLevel.Trace && Debugger.IsAttached))
               .AddFilter($"{nameof(Microsoft)}.{nameof(Microsoft.Extensions)}.{nameof(Microsoft.Extensions.Hosting)}", LogLevel.Warning)
               .AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Information)
               .AddNitroxConsole(options =>
               {
                   options.IsDevMode = builder.Environment.IsDevelopment();
                   options.ColorBehavior = startOptions.IsEmbedded ? LoggerColorBehavior.Disabled : LoggerColorBehavior.Enabled;
               });
        builder.Services.Configure<HostOptions>(options =>
        {
            options.ServicesStartConcurrently = true;
            options.ServicesStopConcurrently = true;
        });
        // Map key-value configuration to types.
        builder.Services
               .AddOptionsWithValidateOnStart<ServerStartOptions, ServerStartOptions.Validator>()
               .BindConfiguration("")
               .Configure(options =>
               {
                   if (string.IsNullOrWhiteSpace(options.GameInstallPath))
                   {
                       options.GameInstallPath = NitroxUser.GamePath;
                   }
                   if (string.IsNullOrWhiteSpace(options.NitroxAssetsPath))
                   {
                       options.NitroxAssetsPath = NitroxUser.AssetsPath;
                   }
                   if (string.IsNullOrWhiteSpace(options.NitroxAppDataPath))
                   {
                       options.NitroxAppDataPath = NitroxUser.AppDataPath;
                   }
               });
        builder.Services.AddOptionsWithValidateOnStart<SubnauticaServerOptions, SubnauticaServerOptions.Validator>()
               .BindConfiguration(SubnauticaServerOptions.CONFIG_SECTION_PATH)
               .Configure(options =>
               {
                   options.Seed = options.Seed switch
                   {
                       null or "" when builder.Environment.IsDevelopment() => "TCCBIBZXAB",
                       null or "" => newWorldSeed.Value,
                       _ => options.Seed
                   };
               });
        // Add initialization services - diagnoses the server environment on startup.
        builder.Services
               .AddHostedSingletonService<PreventMultiServerInitService>()
               .AddHostedSingletonService<NetworkPortAvailabilityService>()
               .AddHostedSingletonService<ServerPerformanceDiagnosticService>()
               .AddKeyedSingleton<Stopwatch>(typeof(ServerPerformanceDiagnosticService), serverStartStopWatch);
        // Add communication services
        builder.Services
               .AddPackets()
               .AddCommands(!startOptions.IsEmbedded);
        // Add APIs - everything else the server will need.
        builder.Services
               .AddSubnauticaEntityManagement()
               .AddSubnauticaResources()
               .AddPersistence() // TODO: Use SQLite instead.
               .AddHibernation()
               .AddHostedSingletonService<GameServerStatusService>()
               .AddHostedSingletonService<PortForwardService>()
               .AddHostedSingletonService<TimeService>()
               .AddHostedSingletonService<PlayerService>()
               .AddHostedSingletonService<StoryTimingService>() // TODO: Merge story services together?
               .AddHostedSingletonService<StoryScheduleService>()
               .AddHostedSingletonService<FmodService>()
               .AddSingleton(_ => GameInfo.Subnautica)
               .AddSingleton<SubnauticaServerProtoBufSerializer>()
               .AddSingleton<NtpSyncer>()
               .AddTransient<SubnauticaServerRandom>();

        await builder.Build().RunAsync();
    }
}
