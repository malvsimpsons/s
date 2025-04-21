using System;
using System.Linq;
using System.Text.Json;
using AssetsTools.NET.Extra;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nitrox.Server.Subnautica.Models.Commands.ArgConverters.Core;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Bases;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities.Spawning;
using Nitrox.Server.Subnautica.Models.Hibernation;
using Nitrox.Server.Subnautica.Models.Packets;
using Nitrox.Server.Subnautica.Models.Persistence.Core;
using Nitrox.Server.Subnautica.Models.Resources;
using Nitrox.Server.Subnautica.Models.Resources.Helper;
using Nitrox.Server.Subnautica.Models.Serialization.Json;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.Packets.Processors.Abstract;
using NitroxServer.GameLogic.Entities;
using NitroxServer.GameLogic.Entities.Spawning;
using ServiceScan.SourceGenerator;

namespace Nitrox.Server.Subnautica.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddHostedSingletonService<T>(this IServiceCollection services) where T : class, IHostedService => services.AddSingleton<T>().AddHostedService(provider => provider.GetRequiredService<T>());

    public static IServiceCollection AddHostedSingletonService<T>(this IServiceCollection services, Func<ServiceProvider, T> factory) where T : class, IHostedService =>
        services.AddSingleton(factory).AddHostedService(provider => provider.GetRequiredService<T>());

    public static IServiceCollection AddPackets(this IServiceCollection services) =>
        services
            .AddHostedSingletonService<LiteNetLibService>()
            .AddHostedSingletonService<PacketService>()
            .AddSingleton<DefaultPacketProcessor>()
            .AddPacketProcessors();

    /// <summary>
    ///     Registers command handlers which will process incoming text-based commands via console or IPC.
    /// </summary>
    public static IServiceCollection AddCommands(this IServiceCollection services, bool hasConsoleInput)
    {
        if (hasConsoleInput)
        {
            services.AddHostedSingletonService<ConsoleCommandService>();
        }
        else
        {
            services.AddHostedSingletonService<IpcCommandService>();
        }
        return services
               .AddHostedSingletonService<CommandService>()
               .AddSingleton<CommandRegistry>()
               .AddSingleton<Func<CommandRegistry>>(provider => provider.GetRequiredService<CommandRegistry>)
               .AddCommandHandlers()
               .AddCommandArgConverters();
    }

    public static IServiceCollection AddSubnauticaEntityManagement(this IServiceCollection services) =>
        services
            .AddHostedSingletonService<BatchEntitySpawnerService>()
            .AddHostedSingletonService<EscapePodService>()
            .AddSingleton<BuildingManager>()
            .AddSingleton<Models.GameLogic.WorldEntityManager>()
            .AddSingleton<IEntityBootstrapperManager, SubnauticaEntityBootstrapperManager>()
            .AddSingleton<SimulationOwnershipData>()
            .AddSingleton<EntitySimulation>()
            .AddSingleton<Models.GameLogic.EntityRegistry>()
            .AddSingleton<BatchEntitySpawnerService>()
            .AddSingleton<EntitySpawnPointFactory, SubnauticaEntitySpawnPointFactory>()
            .AddSingleton<IUweWorldEntityFactory, SubnauticaUweWorldEntityFactory>()
            .AddSingleton<ISimulationWhitelist, SimulationWhitelist>();

    public static IServiceCollection AddSubnauticaResources(this IServiceCollection services) =>
        services
            .AddHostedSingletonService<SubnauticaResourceLoaderService>()
            .AddGameResources()
            .AddSingleton<BatchCellsParser>()
            .AddSingleton<SubnauticaAssetsManager>()
            .AddSingleton<IUwePrefabFactory, SubnauticaUwePrefabFactory>()
            .AddTransient<IMonoBehaviourTemplateGenerator, ThreadSafeMonoCecilTempGenerator>();

    /// <summary>
    ///     Adds server persistence for all defined state known by the server.
    /// </summary>
    public static IServiceCollection AddPersistence(this IServiceCollection services) =>
        services
            .AddHostedSingletonService<PersistenceService>()
            .AddPersistableState()
            .AddSingleton(provider =>
            {
                JsonSerializerOptions options = new()
                {
                    AllowTrailingCommas = true,
                    WriteIndented = provider.GetRequiredService<IHostEnvironment>().IsDevelopment()
                };
                options.Converters.Add(new NitroxIdConverter());
                options.Converters.Add(new TechTypeConverter());
                return options;
            });

    /// <summary>
    ///     Allows the server to go into hibernation when no players are connected.
    /// </summary>
    public static IServiceCollection AddHibernation(this IServiceCollection services) =>
        services.AddHostedSingletonService<HibernationService>()
                .AddHibernators();

    [GenerateServiceRegistrations(AssignableTo = typeof(IStateManager), Lifetime = ServiceLifetime.Singleton, AsSelf = true, AsImplementedInterfaces = true)]
    private static partial IServiceCollection AddPersistableState(this IServiceCollection services);

    [GenerateServiceRegistrations(AssignableTo = typeof(IGameResource), Lifetime = ServiceLifetime.Singleton, AsSelf = true, AsImplementedInterfaces = true)]
    private static partial IServiceCollection AddGameResources(this IServiceCollection services);

    [GenerateServiceRegistrations(AssignableTo = typeof(IHibernate), Lifetime = ServiceLifetime.Singleton)]
    private static partial IServiceCollection AddHibernators(this IServiceCollection services);

    [GenerateServiceRegistrations(AssignableTo = typeof(ICommandHandlerBase), Lifetime = ServiceLifetime.Singleton, CustomHandler = nameof(AddCommandHandler))]
    private static partial IServiceCollection AddCommandHandlers(this IServiceCollection services);

    [GenerateServiceRegistrations(AssignableTo = typeof(IArgConverter), Lifetime = ServiceLifetime.Singleton, AsSelf = true, AsImplementedInterfaces = true)]
    private static partial IServiceCollection AddCommandArgConverters(this IServiceCollection services);

    [GenerateServiceRegistrations(AssignableTo = typeof(PacketProcessor), Lifetime = ServiceLifetime.Singleton)]
    private static partial IServiceCollection AddPacketProcessors(this IServiceCollection services);

    /// <summary>
    ///     Registers a single command and all of its handlers as can be known by the implemented interfaces.
    /// </summary>
    private static void AddCommandHandler<T>(this IServiceCollection services) where T : class, ICommandHandlerBase
    {
        Type[] handlerTypes = typeof(T).GetInterfaces().Where(t => t != typeof(ICommandHandlerBase) && typeof(ICommandHandlerBase).IsAssignableFrom(t)).ToArray();
        if (handlerTypes.Length < 1)
        {
            return;
        }
        services.AddSingleton<T>();

        foreach (Type handlerType in handlerTypes)
        {
            services.AddSingleton(provider =>
            {
                T owner = provider.GetRequiredService<T>();
                return new CommandHandlerEntry(owner, handlerType);
            });
        }
    }
}
