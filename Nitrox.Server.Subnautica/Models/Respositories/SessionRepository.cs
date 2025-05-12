using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Nitrox.Server.Subnautica.Database;
using Nitrox.Server.Subnautica.Database.Models;
using Nitrox.Server.Subnautica.Models.Respositories.Core;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Respositories;

internal class SessionRepository(DatabaseService databaseService, Func<ISessionCleaner[]> sessionCleanersProvider, ILogger<SessionRepository> logger)
{
    private readonly DatabaseService databaseService = databaseService;
    private readonly ILogger<SessionRepository> logger = logger;
    private OrderedDictionary<int, ISessionCleaner> sessionCleaners;

    public async Task<Session> GetOrCreateSessionAsync(string address, ushort port)
    {
        await using WorldDbContext db = await databaseService.GetDbContextAsync();
#if DEBUG
        Stopwatch sw = Stopwatch.StartNew();
#endif
        Session session = await db.GetOrCreateSession(address, port);
#if DEBUG
        sw.Stop();
        logger.ZLogDebug($"{nameof(SessionExtensions.GetOrCreateSession)} took: {sw.Elapsed.TotalMilliseconds}ms");
#endif
        if (session == null)
        {
            logger.ZLogError($"Failed to create session for {address.ToSensitive():@Address}:{port:@Port}");
            return null;
        }
        return session;
    }

    public async Task<bool> SetSessionInactive(SessionId sessionId)
    {
        await using WorldDbContext db = await databaseService.GetDbContextAsync();
        Session session = await db.Sessions
                                  .AsTracking()
                                  .Include(s => s.Connection)
                                  .FirstOrDefaultAsync(s => s.Id == sessionId);
        db.Connections.Remove(session.Connection);
        return await db.SaveChangesAsync() > 0;
    }

    /// <summary>
    ///     Gets the amount of connected sessions.
    /// </summary>
    public async Task<int> GetActiveSessionCount()
    {
        await using WorldDbContext db = await databaseService.GetDbContextAsync();
        return await db.Sessions
                       .Include(s => s.Connection)
                       .Where(s => s.Connection != null)
                       .CountAsync();
    }

    public async Task DeleteSessionAsync(SessionId sessionId)
    {
        await using (WorldDbContext db = await databaseService.GetDbContextAsync())
        {
            Session session = await db.Sessions
                                      .Include(s => s.Player)
                                      .Where(s => s.Id == sessionId)
                                      .FirstOrDefaultAsync();
            if (session == null)
            {
                return;
            }
            if (!await SetSessionInactive(session.Id))
            {
                logger.ZLogWarning($"Failed to set session #{session.Id:@SessionId} inactive");
                return;
            }
            session.Connection = null;
            await RunSessionCleanersAsync(session);

            // Now delete the session and its data (the latter happens through FOREIGN KEY CASCADE DELETE on session id)
            db.Remove(session);
            if (await db.SaveChangesAsync() <= 0)
            {
                logger.ZLogWarning($"Failed to delete session #{sessionId} data");
                return;
            }
        }

        logger.ZLogTrace($"Session #{sessionId} data has been removed");
    }

    /// <summary>
    ///     Lets other APIs migrate session data if necessary (e.g. change database or notify other players of necessary
    ///     changes).
    /// </summary>
    private async Task RunSessionCleanersAsync(Session session)
    {
        sessionCleaners ??= new OrderedDictionary<int, ISessionCleaner>(sessionCleanersProvider().Select(c => new KeyValuePair<int, ISessionCleaner>(c.SessionCleanPriority, c)));
        foreach (ISessionCleaner cleaner in sessionCleaners.Values)
        {
            try
            {
                await cleaner.CleanSessionAsync(session);
            }
            catch (Exception ex)
            {
                logger.ZLogError(ex, $"Error occurred during session cleaning in {cleaner.GetType().Name:@TypeName}");
            }
        }
    }
}
