﻿using Nitrox.Server.Subnautica.Database.Converters;
using Nitrox.Server.Subnautica.Database.Models;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Unity;
using NitroxModel.Networking;

namespace Nitrox.Server.Subnautica.Database;

public sealed class WorldDbContext(DbContextOptions<WorldDbContext> options) : DbContext(options)
{
    public DbSet<Player> Players { get; set; }
    public DbSet<PlayContext> PlayContexts { get; set; }
    public DbSet<Connection> Connections { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<SurvivalContext> PlayerSurvivalStats { get; set; }
    public DbSet<StoryGoal> StoryGoals { get; set; }
    public DbSet<TimeLockedTableIds> TimeLockedTableIds { get; set; }

    /// <summary>
    ///     Batch cells that have been parsed.
    /// </summary>
    public DbSet<BatchCell> BatchCells { get; set; }

    public WorldDbContext() : this(new DbContextOptions<WorldDbContext>())
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        EnableEfMigrations(builder);
        base.OnConfiguring(builder);

        static void EnableEfMigrations(DbContextOptionsBuilder builder)
        {
            // Required for "dotnet ef migrations add ..." to work, shouldn't be used for development/production.
            if (!builder.IsConfigured)
            {
                builder.UseSqlite();
            }
        }
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        base.ConfigureConventions(builder);

        builder
            .Properties<NitroxId>()
            .HaveConversion<NitroxIdConverter>();
        builder
            .Properties<NitroxInt3>()
            .HaveConversion<NitroxInt3Converter>();
        builder
            .Properties<NitroxVector3>()
            .HaveConversion<NitroxVector3Converter>();
        builder
            .Properties<NitroxQuaternion>()
            .HaveConversion<NitroxQuaternionConverter>();
        builder
            .Properties<SessionId>()
            .HaveConversion<SessionIdConverter>();
        builder
            .Properties<PeerId>()
            .HaveConversion<PeerIdIdConverter>();
    }
}
