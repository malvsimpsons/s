using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Nitrox.Server.Subnautica.Database.Core.Interceptors;

internal sealed class DisableInternalEntityFrameworkPragmaInterceptor : IDbCommandInterceptor
{
    private bool done;

    public InterceptionResult<DbCommand> CommandCreating(CommandCorrelatedEventData eventData, InterceptionResult<DbCommand> result)
    {
        if (Interlocked.CompareExchange(ref done, false, false))
        {
            return result;
        }
        if (eventData.Context is not WorldDbContext)
        {
            return InterceptionResult<DbCommand>.SuppressWithResult(PragmaIgnoreDbCommand.Instance);
        }
        Interlocked.Exchange(ref done, true);
        return result;
    }

    private class PragmaIgnoreDbCommand : SqliteCommand
    {
        public static readonly PragmaIgnoreDbCommand Instance = new();

        public override string CommandText
        {
            get => base.CommandText;
            set
            {
                if (value.StartsWith("PRAGMA"))
                {
                    value = $"--{value}";
                }
                base.CommandText = value;
            }
        }

        public override int ExecuteNonQuery()
        {
            if (CommandText.StartsWith("--"))
            {
                return 0;
            }
            return base.ExecuteNonQuery();
        }
    }
}
