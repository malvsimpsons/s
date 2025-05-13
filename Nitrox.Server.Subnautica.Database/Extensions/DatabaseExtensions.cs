using Microsoft.Data.Sqlite;

namespace Nitrox.Server.Subnautica.Database.Extensions;

internal static class DatabaseExtensions
{
    public static SqliteConnection GetOpenSqliteConnection(this WorldDbContext db)
    {
        SqliteConnection connection = (SqliteConnection)db.Database.GetDbConnection();
        connection.Open();
        return connection;
    }
}
