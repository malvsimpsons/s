using System;

namespace NitroxModel.Networking.Packets;

[Serializable]
public class ChatMessage : Packet
{
    public PeerId PlayerId { get; }
    public string Text { get; }

    public ChatMessage(PeerId playerId, string text)
    {
        PlayerId = playerId;
        Text = text;
    }
}
