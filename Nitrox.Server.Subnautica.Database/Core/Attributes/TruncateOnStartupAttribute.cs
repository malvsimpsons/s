namespace Nitrox.Server.Subnautica.Database.Core.Attributes;

/// <summary>
///     Truncates the marked EF entity table when its database is seeded.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
internal class TruncateOnStartupAttribute : Attribute;
