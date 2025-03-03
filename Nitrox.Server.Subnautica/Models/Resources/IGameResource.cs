using System.Threading;
using System.Threading.Tasks;

namespace Nitrox.Server.Subnautica.Models.Resources;

/// <summary>
///     Implementors of this interface are parsing game files and loading its data into memory.
/// </summary>
internal interface IGameResource
{
    Task LoadAsync(CancellationToken cancellationToken);
}
