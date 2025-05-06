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
    private ISessionCleaner[] sessionCleaners;

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
                Id = GetNextSessionId(),
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

    public async Task<PlayerSession> GetSessionAsync(PeerId peerId)
    {
        await using WorldDbContext db = await databaseService.GetDbContextAsync();
        return await db.PlayerSessions
                       .Include(s => s.Player)
                       .Where(s => s.Player.Id == peerId)
                       .FirstOrDefaultAsync();
    }

    public async Task DeleteSessionAsync(SessionId sessionId)
    {
        await using (WorldDbContext db = await databaseService.GetDbContextAsync())
        {
            PlayerSession session = await db.PlayerSessions
                                            .Include(s => s.Player)
                                            .Where(s => s.Id == sessionId)
                                            .FirstOrDefaultAsync();
            if (session == null)
            {
                return;
            }

            await RunSessionCleanersAsync(session);

            // Now delete the session and its data (the latter happens through FOREIGN KEY CASCADE DELETE on session id)
            db.Remove(session);
            if (await db.SaveChangesAsync() <= 0)
            {
                logger.LogWarning("Failed to delete session data on session id {SessionId}", sessionId);
                return;
            }
        }

        MarkSessionIdAsUsed(sessionId);
        logger.LogTrace("Session #{SessionId} data has been removed", sessionId);
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
    private async Task RunSessionCleanersAsync(PlayerSession session)
    {
        sessionCleaners ??= sessionCleanersProvider();
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
