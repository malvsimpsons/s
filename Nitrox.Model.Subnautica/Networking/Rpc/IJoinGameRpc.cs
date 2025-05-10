using System.Threading.Tasks;

namespace Nitrox.Model.Subnautica.Networking.Rpc;

public interface IJoinGameRpc
{
    Task<RequestResult> RequestPolicy();
    
    public record RequestResult
    {
        public int I { get; private set; }
    }
}
