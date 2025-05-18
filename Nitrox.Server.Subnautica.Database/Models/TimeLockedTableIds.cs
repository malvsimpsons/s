using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Nitrox.Server.Subnautica.Database.Core.Attributes;

namespace Nitrox.Server.Subnautica.Database.Models;

/// <summary>
///     Used within SQL as a custom <c>auto increment</c> to prevent IDs being reused too soon.
/// </summary>
/// <remarks>
///     <see cref="TableName" /> property is at the bottom as it's an "enum" column. Performance is better this way, see
///     <a href="https://sqlite.org/queryplanner-ng.html">SQLite Query Planner</a> for more info.
/// </remarks>
[Table("_timeLockedTableIds")]
[Index(nameof(Id), nameof(TableName), IsUnique = true)]
[TruncateOnStartup]
public class TimeLockedTableIds
{
    /// <summary>
    ///     The ID to lock.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    ///     System uptime in milliseconds till ID becomes available.
    /// </summary>
    [DefaultValue("uptime('0:10:0')")] // Locks IDs by 10 minutes.
    public long TimeTillUnlock { get; set; }

    /// <summary>
    ///     Name of the table that uses the ID.
    /// </summary>
    public string TableName { get; set; }
}
