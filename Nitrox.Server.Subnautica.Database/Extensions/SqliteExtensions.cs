using SQLitePCL;
using static SQLitePCL.raw;

namespace Nitrox.Server.Subnautica.Database.Extensions;

internal static class SqliteExtensions
{
    public static SqliteReader Query(this sqlite3 sqlite, ReadOnlySpan<byte> sql, params ReadOnlySpan<object> parameters)
    {
        sqlite3_stmt stmt = null;
        while (!sql.IsEmpty)
        {
            if (sqlite3_prepare_v2(sqlite, sql, out stmt, out sql) != SQLITE_OK)
            {
                return default;
            }
            if (sqlite3_bind_parameter_count(stmt) > 0)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    stmt.Bind(i + 1, parameters[i]);
                }
            }
            int rc = sqlite3_step(stmt);
            if (rc is not (SQLITE_OK or SQLITE_DONE or SQLITE_ROW))
            {
                throw new Exception($"Sqlite error: {sqlite3_errmsg(sqlite).utf8_to_string()}");
            }
            if (rc == SQLITE_ROW)
            {
                return new(sqlite, stmt);
            }
        }
        return default;
    }

    public readonly struct SqliteReader(sqlite3 sqlite, sqlite3_stmt stmt) : IDisposable
    {
        public bool IsValid { get; } = stmt is not null;

        public int Next()
        {
            if (!IsValid)
            {
                return SQLITE_DONE;
            }
            int rc = sqlite3_step(stmt);
            if (rc is not (SQLITE_OK or SQLITE_DONE or SQLITE_ROW))
            {
                throw new Exception($"Sqlite error: {sqlite3_errmsg(sqlite).utf8_to_string()}");
            }
            return rc;
        }

        public T Read<T>(int index)
        {
            if (!IsValid)
            {
                return default;
            }
            Type type = typeof(T);
            switch (type)
            {
                case not null when type == typeof(int):
                    return (T)(object)sqlite3_column_int(stmt, index);
                case not null when type == typeof(long):
                    return (T)(object)sqlite3_column_int64(stmt, index);
                case not null when type == typeof(string):
                    return (T)(object)sqlite3_column_text(stmt, index).utf8_to_string();
                default:
                    return default;
            }
        }

        public void Dispose()
        {
            if (!IsValid)
            {
                return;
            }
            while (Next() is not SQLITE_DONE)
            {
            }
            stmt.Dispose();
        }
    }
}
