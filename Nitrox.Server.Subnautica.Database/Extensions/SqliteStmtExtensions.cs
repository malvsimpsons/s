using SQLitePCL;
using static SQLitePCL.raw;

namespace Nitrox.Server.Subnautica.Database.Extensions;

internal static class SqliteStmtExtensions
{
    public static int Bind<T>(this sqlite3_stmt stmt, int index, T obj)
    {
        switch (obj)
        {
            case short s:
                return sqlite3_bind_int(stmt, index, s);
            case ushort u:
                return sqlite3_bind_int(stmt, index, u);
            case int i:
                return sqlite3_bind_int(stmt, index, i);
            case long l:
                return sqlite3_bind_int64(stmt, index, l);
            case string s:
                return sqlite3_bind_text(stmt, index, s);
            case DBNull n:
                return sqlite3_bind_null(stmt, index);
            default:
                throw new NotSupportedException($"Unsupported object type for bind: {obj?.GetType().Name ?? "null"}");
        }
    }
}
