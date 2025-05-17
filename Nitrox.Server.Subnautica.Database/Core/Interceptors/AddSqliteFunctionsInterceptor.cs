using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Nitrox.Server.Subnautica.Database.Core.Interceptors;

internal sealed class AddSqliteFunctionsInterceptor : IDbConnectionInterceptor
{
    private DbConnection lastConnection;

    public void ConnectionOpened(DbConnection baseConnection, ConnectionEndEventData eventData)
    {
        if (Interlocked.CompareExchange(ref lastConnection, baseConnection, lastConnection) == baseConnection)
        {
            return;
        }
        if (baseConnection is not SqliteConnection connection)
        {
            return;
        }

        connection.CreateFunction("uptime", () => Environment.TickCount64);
        connection.CreateFunction("uptime", (string offset) =>
        {
            if (!TimeSpan.TryParse(offset, out TimeSpan offsetTs))
            {
                offsetTs = TimeSpan.Zero;
            }
            return offsetTs.Add(TimeSpan.FromMilliseconds(Environment.TickCount64)).TotalMilliseconds;
        });
    }

    public Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = new())
    {
        ConnectionOpened(connection, eventData);
        return Task.CompletedTask;
    }
}
