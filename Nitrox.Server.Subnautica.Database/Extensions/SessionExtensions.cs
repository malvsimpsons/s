using System.Text;
using Microsoft.Data.Sqlite;
using Nitrox.Server.Subnautica.Database.Models;
using NitroxModel.Networking;
using SQLitePCL;
using static SQLitePCL.raw;

namespace Nitrox.Server.Subnautica.Database.Extensions;

public static class SessionExtensions
{
    // language=sql
    private const string GET_SESSION_SQL =
        """
        SELECT s.Id, s.PlayerId
        FROM Sessions s
        LEFT JOIN Connections c ON c.Id = s.ConnectionId
        WHERE c.Address = @address AND c.Port = @port
        LIMIT 1
        """;

    // language=sql
    private const string CREATE_SESSION_SQL =
        $"""
        BEGIN;
        
        INSERT OR ROLLBACK INTO Connections(Id, Address, Port)
        VALUES
            (
                (
                    -- Try get first unlocked & smallest ID, or next ID
                    WITH LockedSessionIds AS (
                        SELECT Id, TimeTillUnlock
                        FROM _timeLockedTableIds
                        WHERE TableName = 'Sessions'
                    )
                    SELECT IFNULL(MIN(Id), (SELECT COUNT(*) + 1 FROM LockedSessionIds)) as Id
                    FROM LockedSessionIds
                    WHERE TimeTillUnlock <= datetime('now', 'subsec')
                    ORDER BY Id, TimeTillUnlock
                    LIMIT 1
                ),
                @address,
                @port
            );
        
        INSERT OR ROLLBACK INTO Sessions(Id, ConnectionId)
        VALUES
            (
                last_insert_rowid(),
                last_insert_rowid()
            );
        
        DELETE FROM _timeLockedTableIds
        WHERE Id = last_insert_rowid() AND TableName = 'Sessions';
        
        END;

        {GET_SESSION_SQL}
        """;

    private static readonly byte[] getSessionSqlUtf8 = Encoding.UTF8.GetBytes(GET_SESSION_SQL);
    private static readonly byte[] createSessionSqlUtf8 = Encoding.UTF8.GetBytes(CREATE_SESSION_SQL);

    /// <summary>
    ///     Gets or inserts new records for session and gets the (pre-existing) session record.
    /// </summary>
    /// <remarks>
    ///     This Sqlite query is optimized as it is used for every incoming packet.
    /// </remarks>
    public static Session GetOrCreateSession(this WorldDbContext db, string address, ushort port)
    {
        SqliteConnection connection = db.GetOpenSqliteConnection(); // Don't dispose; managed by EF Core.
        if (sqlite3_prepare_v2(connection.Handle, getSessionSqlUtf8, out sqlite3_stmt stmt) != SQLITE_OK)
        {
            return null;
        }
        using (stmt)
        {
            sqlite3_bind_text16(stmt, 1, address);
            sqlite3_bind_int(stmt, 2, port);
            if (sqlite3_step(stmt) == SQLITE_ROW)
            {
                return new Session
                {
                    Id = (SessionId)sqlite3_column_int(stmt, 0),
                    Player = new Player { Id = (PeerId)sqlite3_column_int64(stmt, 1) }
                };
            }
        }

        // Fallback: create session
        using SqliteExtensions.SqliteReader reader = connection.Handle.Query(createSessionSqlUtf8, address, port);
        if (!reader.IsValid)
        {
            return null;
        }
        int sessionId = reader.Read<int>(0);
        if (sessionId < 1)
        {
            throw new Exception("Session ID must be larger than 0");
        }
        Player player = null;
        if (reader.Read<long>(1) is var playerId and > 0)
        {
            player = new Player { Id = (PeerId)playerId };
        }
        Session session = new()
        {
            Id = (SessionId)sessionId,
            Player = player,
            Connection = new Connection
            {
                Address = address,
                Port = port
            }
        };
        return session;
    }
}
