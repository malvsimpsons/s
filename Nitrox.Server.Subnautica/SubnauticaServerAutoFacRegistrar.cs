using System;
using Autofac;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities.Spawning;
using Nitrox.Server.Subnautica.Models.Serialization;
using NitroxServer;
using NitroxServer.GameLogic.Entities;
using NitroxServer.GameLogic.Entities.Spawning;
using NitroxServer.Serialization;

namespace Nitrox.Server.Subnautica;

// TODO: REMOVE THIS
[Obsolete("Use .NET Generic Host in Nitrox.Server.Subnautica")]
public class SubnauticaServerAutoFacRegistrar : ServerAutoFacRegistrar
{
    public override void RegisterDependencies(ContainerBuilder containerBuilder)
    {
        base.RegisterDependencies(containerBuilder);

        containerBuilder.RegisterType<SimulationWhitelist>()
                        .As<ISimulationWhitelist>()
                        .SingleInstance();
        containerBuilder.Register(c => new SubnauticaServerProtoBufSerializer(
                                  ))
                        .As<ServerProtoBufSerializer, IServerSerializer>()
                        .SingleInstance();
        containerBuilder.Register(c => new SubnauticaServerJsonSerializer())
                        .As<ServerJsonSerializer, IServerSerializer>()
                        .SingleInstance();

        containerBuilder.RegisterType<SubnauticaEntitySpawnPointFactory>().As<EntitySpawnPointFactory>().SingleInstance();

        // ResourceAssets resourceAssets = ResourceAssetsParser.Parse();

        // containerBuilder.Register(c => resourceAssets).SingleInstance();
        // containerBuilder.Register(c => resourceAssets.WorldEntitiesByClassId).SingleInstance();
        // containerBuilder.Register(c => resourceAssets.PrefabPlaceholdersGroupsByGroupClassId).SingleInstance();
        // containerBuilder.Register(c => resourceAssets.NitroxRandom).SingleInstance();
        // containerBuilder.RegisterType<SubnauticaUweWorldEntityFactory>().As<IUweWorldEntityFactory>().SingleInstance();
        //
        // SubnauticaUwePrefabFactory prefabFactory = new SubnauticaUwePrefabFactory(resourceAssets.LootDistributionsJson);
        // containerBuilder.Register(c => prefabFactory).As<IUwePrefabFactory>().SingleInstance();
        // containerBuilder.RegisterType<SubnauticaEntityBootstrapperManager>()
        //                 .As<IEntityBootstrapperManager>()
        //                 .SingleInstance();
        //
        // containerBuilder.RegisterType<SubnauticaMap>().As<IMap>().InstancePerLifetimeScope();
        // containerBuilder.RegisterType<Models.GameLogic.EntityRegistry>().AsSelf().InstancePerLifetimeScope();
        // containerBuilder.RegisterType<SubnauticaWorldModifier>().As<IWorldModifier>().InstancePerLifetimeScope();
        // containerBuilder.Register(c => FMODWhitelist.Load(GameInfo.Subnautica)).InstancePerLifetimeScope();
        //
        // containerBuilder.Register(_ => new RandomSpawnSpoofer(resourceAssets.RandomPossibilitiesByClassId))
        //                 .SingleInstance();
    }
}
