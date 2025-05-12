using System.Data;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using Nitrox.Server.Subnautica.Database.Models;
using NitroxModel.Networking;

namespace Nitrox.Server.Subnautica.Database.Extensions;

public static class SessionExtensions
{
    private static SqliteCommand getOrCreateSessionCommand;

    /// <summary>
    ///     Gets or inserts new records for session and gets the (pre-existing) session record.
    /// </summary>
    /// <remarks>
    ///     This Sqlite query is highly optimized as it is used for every incoming packet.
    /// </remarks>
    public static async Task<Session> GetOrCreateSession(this WorldDbContext db, string address, ushort port)
    {
        // TODO: Lock down session id so it isn't reused within 10 minutes.
        bool isFirst = false;
        if (getOrCreateSessionCommand == null)
        {
            isFirst = true;
            getOrCreateSessionCommand = new SqliteCommand();
            getOrCreateSessionCommand.CommandText = """
                                                    BEGIN;

                                                    INSERT OR IGNORE INTO Connections (Address, Port, Created)
                                                    VALUES (@address, @port, @created);

                                                    INSERT OR IGNORE INTO Sessions (ConnectionId, PlayerId)
                                                    VALUES (
                                                               (
                                                                   SELECT coalesce(Id, last_insert_rowid())
                                                                   FROM Connections
                                                                   WHERE Address = @address AND Port = @port
                                                               ),
                                                               NULL
                                                           );

                                                    END;

                                                    SELECT s.Id, s.ConnectionId, c.Address, c.Port, c.Created, s.PlayerId
                                                    FROM Sessions s
                                                    LEFT JOIN Connections c ON c.Id = s.ConnectionId
                                                    WHERE c.Address = @address AND c.Port = @port;
                                                    """;
            getOrCreateSessionCommand.Parameters.AddWithValue("address", address);
            getOrCreateSessionCommand.Parameters.AddWithValue("port", port);
            getOrCreateSessionCommand.Parameters.AddWithValue("created", DateTimeOffset.Now);
            getOrCreateSessionCommand.UpdatedRowSource = UpdateRowSource.FirstReturnedRecord;
        }

        DbConnection dbConnection = db.Database.GetDbConnection();
        await dbConnection.OpenAsync();
        getOrCreateSessionCommand.Connection = (SqliteConnection)dbConnection;
        if (isFirst)
        {
            getOrCreateSessionCommand.Prepare();
        }

        getOrCreateSessionCommand.Parameters[0].Value = address;
        getOrCreateSessionCommand.Parameters[1].Value = port;
        getOrCreateSessionCommand.Parameters[2].Value = DateTimeOffset.Now;

        await using DbDataReader reader = await getOrCreateSessionCommand.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            Session session = new()
            {
                Id = (SessionId)reader.GetInt32(0),
                Connection = new Connection
                {
                    Id = reader.GetInt32(1),
                    Address = reader.GetString(2),
                    Port = (ushort)reader.GetInt32(3),
                    Created = DateTimeOffset.Parse(reader.GetString(4))
                }
            };
            if (!reader.IsDBNull(5))
            {
                session.Player = new Player { Id = (PeerId)reader.GetInt64(5) };
            }
            return session;
        }
        return null;
    }
}
