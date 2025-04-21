using System;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Networking.Packets;

[Serializable]
public class PermsChanged : Packet
{
    public Perms NewPerms;

    public PermsChanged(Perms newPerms)
    {
        NewPerms = newPerms;
    }
}
