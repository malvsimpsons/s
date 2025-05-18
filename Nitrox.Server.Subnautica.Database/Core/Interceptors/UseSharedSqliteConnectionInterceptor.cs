using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Nitrox.Server.Subnautica.Database.Core.Interceptors;

public class UseSharedSqliteConnectionInterceptor : IDbCommandInterceptor
{
    private static SqliteConnection connection;

    public static SqliteConnection Connection
    {
        get
        {
            if (connection != null)
            {
                return connection;
            }
            connection = new SqliteConnection("DataSource=:memory:");
            connection.CreateFunction("uptime", () => Environment.TickCount64);
            connection.CreateFunction("uptime", (string offset) =>
            {
                if (!TimeSpan.TryParse(offset, out TimeSpan offsetTs))
                {
                    offsetTs = TimeSpan.Zero;
                }
                return offsetTs.Add(TimeSpan.FromMilliseconds(Environment.TickCount64)).TotalMilliseconds;
            });
            connection.Open();
            return connection;
        }
    }

    public InterceptionResult<DbCommand> CommandCreating(CommandCorrelatedEventData eventData, InterceptionResult<DbCommand> result)
    {
        return InterceptionResult<DbCommand>.SuppressWithResult(new SqliteCommand { Connection = Connection });
    }

    public DbCommand CommandCreated(CommandEndEventData eventData, DbCommand result)
    {
        result.Connection = Connection;
        return result;
    }
}
