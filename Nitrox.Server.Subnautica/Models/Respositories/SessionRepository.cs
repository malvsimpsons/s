using System.Linq;
using Microsoft.EntityFrameworkCore;
using Nitrox.Server.Subnautica.Database;
using Nitrox.Server.Subnautica.Database.Models;
using Nitrox.Server.Subnautica.Models.Events;
using Nitrox.Server.Subnautica.Models.Events.Core;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Respositories;

internal class SessionRepository(DatabaseService databaseService, ITrigger<ISeeSessionDisconnected, Session> sessionDisconnectedTrigger, ILogger<SessionRepository> logger) : ISeeDbInitialized
{
    private readonly DatabaseService databaseService = databaseService;
    private readonly ILogger<SessionRepository> logger = logger;
    private readonly ITrigger<ISeeSessionDisconnected, Session> sessionDisconnectedTrigger = sessionDisconnectedTrigger;

    public async Task<Session> GetOrCreateSessionAsync(string address, ushort port)
    {
        await using WorldDbContext db = await databaseService.GetDbContextAsync();
        Session session = db.GetOrCreateSession(address, port);
        if (session == null)
        {
            logger.ZLogError($"Failed to create session for {address.ToSensitive():@Address}:{port:@Port}");
            return null;
        }
        return session;
    }

    /// <summary>
    ///     Gets the amount of connected sessions.
    /// </summary>
    public async Task<int> GetActiveSessionCount()
    {
        await using WorldDbContext db = await databaseService.GetDbContextAsync();
        return await db.Sessions
                       .Where(s => s.Connection != null)
                       .CountAsync();
    }

    public async Task DeleteSessionAsync(SessionId sessionId)
    {
        await using (WorldDbContext db = await databaseService.GetDbContextAsync())
        {
            Session session = await db.Sessions
                                      .AsTracking()
                                      .Include(s => s.Player)
                                      .Include(session => session.Connection)
                                      .Where(s => s.Id == (ushort)sessionId)
                                      .FirstOrDefaultAsync();
            if (session == null)
            {
                return;
            }
            if (session.Connection != null)
            {
                db.Connections.Remove(session.Connection);
                session.Connection = null;
                await db.SaveChangesAsync();
            }
            await sessionDisconnectedTrigger.Trigger(session);

            // Now delete the session and its data (the latter happens through FOREIGN KEY CASCADE DELETE on session id)
            db.TimeLockedTableIds.Add(new TimeLockedTableIds
            {
                Id = sessionId,
                TableName = nameof(db.Sessions)
            });
            db.Remove(session);
            if (await db.SaveChangesAsync() <= 0)
            {
                logger.ZLogWarning($"Failed to delete session #{sessionId} data");
                return;
            }
        }

        logger.ZLogTrace($"Session #{sessionId} data has been removed");
    }

    public async Task<SessionId[]> GetSessionIds()
    {
        await using WorldDbContext db = await databaseService.GetDbContextAsync();
        return await db.Sessions
                       .Include(s => s.Connection)
                       .Select(s => s.Id)
                       .ToArrayAsync();
    }

    public async ValueTask HandleDatabaseInitialized()
    {
        // Cleanup left-over work from previous server instance, if necessary.
        foreach (SessionId sessionId in await GetSessionIds())
        {
            await DeleteSessionAsync(sessionId);
        }
    }
}
