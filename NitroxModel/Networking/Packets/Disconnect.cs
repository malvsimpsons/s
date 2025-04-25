using System;

namespace NitroxModel.Networking.Packets;

[Serializable]
public class Disconnect : Packet
{
    public Disconnect(SessionId sessionId)
    {
        SessionId = sessionId;
    }

    public SessionId SessionId { get; }
}
