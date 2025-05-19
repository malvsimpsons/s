using System;
using NitroxModel.DataStructures.Util;

namespace NitroxModel.Networking.Session;

[Serializable]
public class AuthenticationContext(Optional<byte[]> playerLoginKey, Optional<string> serverPassword)
{
    public Optional<byte[]> PlayerLoginKey { get; } = playerLoginKey;
    public Optional<string> ServerPassword { get; } = serverPassword;
}
