using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Nitrox.Server.Subnautica.Database;
using Nitrox.Server.Subnautica.Database.Models;
using Nitrox.Server.Subnautica.Models.Respositories.Core;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Respositories;

internal class SessionRepository(DatabaseService databaseService, Func<ISessionCleaner[]> sessionCleanersProvider, ILogger<SessionRepository> logger)
{
    /// <summary>
    ///     Minutes to wait before letting a session id be reused.
    /// </summary>
    /// <remarks>
    ///     It's important to give plenty of time for the server to finish any processing. Otherwise, impersonation issues can
    ///     happen.
    /// </remarks>
    private const int SESSION_ID_REUSE_LOCK_IN_MINUTES = 10;

    private readonly DatabaseService databaseService = databaseService;
    private readonly ILogger<SessionRepository> logger = logger;
    private readonly Lock sessionIdLock = new();

    /// <summary>
    ///     Stopwatch is used instead of <see cref="DateTimeOffset.UtcNow" /> so session logic is independent of system time.
    ///     Otherwise, if host changes system time, session could be reused earlier than expected.
    /// </summary>
    private readonly Stopwatch sessionStopWatch = Stopwatch.StartNew();

    private readonly SortedList<TimeSpan, SessionId> usedSessionIds = [];
    private SessionId nextSessionId = 1;
    private OrderedDictionary<int, ISessionCleaner> sessionCleaners;

    public async Task<Session> GetOrCreateSessionAsync(string address, ushort port)
    {
        await using WorldDbContext db = await databaseService.GetDbContextAsync();
        Session session = await db.Sessions
                                  .Include(s => s.Player)
                                  .Include(s => s.Connection)
                                  .Where(s => s.Connection.Address == address && s.Connection.Port == port)
                                  .FirstOrDefaultAsync();
        if (session == null)
        {
            session = new Session
            {
                Id = GetNextSessionId(),
                Connection = new Connection
                {
                    Address = address,
                    Port = port
                }
            };
            db.Sessions.Add(session);
            if (await db.SaveChangesAsync() < 1)
            {
                logger.ZLogError($"Failed to create session for {address.ToSensitive():@Address}:{port:@Port}");
                return null;
            }
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

        MarkSessionIdAsUsed(sessionId);
        logger.ZLogTrace($"Session #{sessionId} data has been removed");
    }

    private SessionId GetNextSessionId()
    {
        lock (sessionIdLock)
        {
            (TimeSpan timeThreshold, SessionId sessionId) = usedSessionIds.FirstOrDefault();
            if (sessionId > 0 && timeThreshold <= sessionStopWatch.Elapsed)
            {
                usedSessionIds.Remove(timeThreshold);
                return sessionId;
            }

            return nextSessionId++;
        }
    }

    /// <summary>
    ///     Blocks the session id from being reused until later.
    /// </summary>
    private void MarkSessionIdAsUsed(SessionId sessionId)
    {
        lock (sessionIdLock)
        {
            if (usedSessionIds.ContainsValue(sessionId))
            {
                throw new Exception($"Tried to add duplicate session id {sessionId} to used session ids");
            }
            usedSessionIds.Add(sessionStopWatch.Elapsed.Add(TimeSpan.FromMinutes(SESSION_ID_REUSE_LOCK_IN_MINUTES)), sessionId);
        }
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
