namespace Nitrox.Server.Subnautica.Models.Hibernation;

/// <summary>
///     Implementors (which do constant work) can sleep and resume.
/// </summary>
public interface IHibernate
{
    Task Hibernate();
    Task Resume();
}
