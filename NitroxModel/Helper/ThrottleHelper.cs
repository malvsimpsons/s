using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;

namespace NitroxModel.Helper;

public static class ThrottleHelper
{
    private static readonly ConcurrentDictionary<Entry, CancellationTokenSource> entries = [];

    /// <summary>
    ///     Throttles the call for the given time. Calls it immediately if not throttling yet.
    /// </summary>
    public static CancellationToken Throttle(TimeSpan throttleTime, Action call, [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0) =>
        entries.AddOrUpdate(new Entry(sourceFilePath, sourceLineNumber, new EntryContext(false, call, throttleTime)), AddValueFactory, UpdateValueFactory)?.Token ?? default;

    private static CancellationTokenSource AddValueFactory(Entry entry)
    {
        entry.Context.Call();
        CancellationTokenSource cts = new(entry.Context.ThrottleTime);
        cts.Token.Register(() =>
        {
            cts.Dispose();
        });
        return cts;
    }

    private static CancellationTokenSource UpdateValueFactory(Entry entry, CancellationTokenSource source)
    {
        if (!source.IsCancellationRequested && !entry.Context.Updated)
        {
            entry.Context.Updated = true;
            source.Token.Register(entry.Context.Call);
            return source;
        }
        CancellationTokenSource cts = new(entry.Context.ThrottleTime);
        cts.Token.Register(() => cts.Dispose());
        return cts;
    }

    private readonly record struct Entry(string SourceFilePath, int SourceLineNumber, EntryContext Context)
    {
        public bool Equals(Entry other) => SourceFilePath == other.SourceFilePath && SourceLineNumber == other.SourceLineNumber;

        public override int GetHashCode()
        {
            unchecked
            {
                return ((SourceFilePath != null ? SourceFilePath.GetHashCode() : 0) * 397) ^ SourceLineNumber;
            }
        }
    }

    private record EntryContext(bool Updated, Action Call, TimeSpan ThrottleTime)
    {
        public bool Updated { get; set; } = Updated;
    }
}
