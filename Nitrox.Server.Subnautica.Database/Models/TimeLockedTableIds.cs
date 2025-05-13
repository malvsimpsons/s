using System.ComponentModel.DataAnnotations.Schema;

namespace Nitrox.Server.Subnautica.Database.Models;

/// <summary>
///     Used within SQL as a custom <c>auto increment</c> to prevent IDs being reused too soon.
/// </summary>
[Table("_timeLockedTableIds")]
[Index(nameof(TableName), nameof(Id), IsUnique = true)]
public class TimeLockedTableIds
{
    /// <summary>
    ///     Name of the table that uses the ID.
    /// </summary>
    public string TableName { get; set; }

    /// <summary>
    ///     The ID to lock.
    /// </summary>
    public int Id { get; set; }

    public DateTimeOffset TimeTillUnlock { get; set; }
}
