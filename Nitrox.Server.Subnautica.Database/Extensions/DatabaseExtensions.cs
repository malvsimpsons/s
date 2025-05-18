using Microsoft.Data.Sqlite;

namespace Nitrox.Server.Subnautica.Database.Extensions;

public static class DatabaseExtensions
{
    public static bool SqliteSave(this WorldDbContext db, string filePath) => LoadOrSave(db, filePath);
    public static bool SqliteLoad(this WorldDbContext db, string filePath) => LoadOrSave(db, filePath, false);

    /// <summary>
    ///     Loads a backed-up database file into the active memory database.
    /// </summary>
    /// <remarks>
    ///     See <a href="https://sqlite.org/backup.html">Sqlite backup documentation</a> for more info.
    /// </remarks>
    private static bool LoadOrSave(this WorldDbContext db, string filePath, bool isSave = true)
    {
        sqlite3 memHandle = db.GetSqlite3Handle();
        sqlite3 fileHandle = null;
        try
        {
            int flags = isSave ? SQLITE_OPEN_READWRITE | SQLITE_OPEN_CREATE : SQLITE_OPEN_READONLY;
            flags |= SQLITE_OPEN_URI;
            if (sqlite3_open_v2(filePath, out fileHandle, flags, null) != SQLITE_OK)
            {
                return false;
            }
            sqlite3 sourceDb = isSave ? memHandle : fileHandle;
            sqlite3 destDb = isSave ? fileHandle : memHandle;
            using sqlite3_backup backup = sqlite3_backup_init(destDb, "main", sourceDb, "main");
            if (backup.IsInvalid)
            {
                return false;
            }
            sqlite3_backup_step(backup, -1);
            sqlite3_backup_finish(backup);
            if (sqlite3_errcode(memHandle) != SQLITE_OK)
            {
                throw new Exception(sqlite3_errmsg(destDb).utf8_to_string());
            }
            return true;
        }
        finally
        {
            if (fileHandle != null)
            {
                sqlite3_close(fileHandle);
            }
        }
    }

    public static sqlite3 GetSqlite3Handle(this WorldDbContext db) => ((SqliteConnection)db.Database.GetDbConnection()).Handle;
}
