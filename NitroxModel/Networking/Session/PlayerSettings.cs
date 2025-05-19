using System;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.Networking.Session;

[Serializable]
public class PlayerSettings(string username, NitroxColor playerColor)
{
    public string Username { get; } = username;

    public NitroxColor PlayerColor { get; } = playerColor;

    public override string ToString()
    {
        return $"[{nameof(PlayerSettings)}: Username: {Username}, {nameof(PlayerSettings)}: PlayerColor: {PlayerColor}]";
    }
}
