using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nitrox.Server.Subnautica.Database;
using Nitrox.Server.Subnautica.Database.Models;
using Nitrox.Server.Subnautica.Models.Respositories.Core;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Respositories;

internal class SessionRepository(DatabaseService databaseService, IEnumerable<ISessionCleaner> sessionCleaners, ILogger<SessionRepository> logger)
{
    private readonly DatabaseService databaseService = databaseService;
    private readonly ILogger<SessionRepository> logger = logger;
    private readonly ISessionCleaner[] sessionCleaners = [..sessionCleaners];

    public async Task<PlayerSession> GetOrCreateSessionAsync(string address, ushort port)
    {
        await using WorldDbContext db = await databaseService.GetDbContextAsync();
        PlayerSession playerSession = await db.PlayerSessions
                                              .AsTracking()
                                              .Include(s => s.Player)
                                              .Where(s => s.Address == address && s.Port == port)
                                              .FirstOrDefaultAsync();
        if (playerSession == null)
        {
            playerSession = new PlayerSession
            {
                Address = address,
                Port = port
            };
            db.PlayerSessions.Add(playerSession);
            if (await db.SaveChangesAsync() != 1)
            {
                logger.LogError("Failed to create session for {Address}:{Port}", address, port);
                return null;
            }
        }

        return playerSession;
    }

    public async Task DeleteSessionAsync(string address, ushort port)
    {
        await using (WorldDbContext db = await databaseService.GetDbContextAsync())
        {
            PlayerSession session = await db.PlayerSessions
                                            .Include(s => s.Player)
                                            .Where(s => s.Address == address && s.Port == port)
                                            .FirstOrDefaultAsync();
            if (session == null)
            {
                return;
            }
            if (session.Player != null && session.Player.Id > 0)
            {
                await RunSessionCleanersAsync(session);
            }

            // Now delete the session and its data (happens through FOREIGN KEY CASCADE DELETE on session id)
            db.Remove(session);
            if (await db.SaveChangesAsync() <= 0)
            {
                logger.LogWarning("Failed to delete session data for {Address}:{Port}", address, port);
                return;
            }
            logger.LogDebug("Session data for disconnected client {Address}:{Port} has been removed", address, port);
        }
    }

    /// <summary>
    ///     Lets other APIs migrate session data if necessary (e.g. change database or notify other players of necessary
    ///     changes).
    /// </summary>
    private async Task RunSessionCleanersAsync(PlayerSession session)
    {
        foreach (ISessionCleaner cleaner in sessionCleaners)
        {
            try
            {
                await cleaner.CleanSessionAsync(session);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred during session cleaning in {TypeName}", cleaner.GetType().Name);
            }
        }
    }
}
