using System;

namespace NitroxModel.Networking.Packets;

[Serializable]
public class ChatMessage : Packet
{
    public SessionId PlayerId { get; }
    public string Text { get; }

    public ChatMessage(SessionId playerId, string text)
    {
        PlayerId = playerId;
        Text = text;
    }
}
