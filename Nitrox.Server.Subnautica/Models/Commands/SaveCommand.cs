﻿using System;
using System.ComponentModel;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.DataStructures.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.MODERATOR)]
internal class SaveCommand(DatabaseService databaseService) : ICommandHandler
{
    private readonly DatabaseService databaseService = databaseService;

    [Description("Saves the world")]
    public async Task Execute(ICommandContext context)
    {
        await context.MessageAllAsync("World is saving...");
        // TODO: Backup config file
        // TODO: Change this to use options (e.g. MaxBackups)
        await databaseService.BackupAsync(DateTimeOffset.Now.ToString("O").Replace("T", " "));
    }
}
