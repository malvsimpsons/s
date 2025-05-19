using NitroxModel.Networking.Packets;
using NitroxModel.Networking.Session;

namespace NitroxClient.Communication.Abstract
{
    public interface IMultiplayerSessionState
    {
        IClient Client { get; }
        string IpAddress { get; }
        int ServerPort { get; }
        SessionPolicy SessionPolicy { get; }
        PlayerSettings PlayerSettings { get; }
        AuthenticationContext AuthenticationContext { get; }
        SessionReservation Reservation { get; }
    }
}
