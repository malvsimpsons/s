using System;
using System.Text.RegularExpressions;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.Helper;

public static class PlayerNameHelper
{
    /// <remarks>
    ///     <a href="https://regex101.com/r/eTWiEs/2/">Test pattern on regex101.com</a>
    /// </remarks>
    public static bool IsValidPlayerName(string name)
    {
        name = CleanPlayerName(name);
        return Regex.IsMatch(name, @"^[a-z0-9._\- ]{3,25}$", RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);
    }

    public static string CleanPlayerName(string name)
    {
        if (name is null)
        {
            return "";
        }
        return name.Trim();
    }

    public static NitroxColor GenerateColorByName(string name)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            byte[] nameHash = name.AsMd5Hash();
            if (nameHash.Length >= 8)
            {
                float hue = BitConverter.ToUInt64([nameHash[0], nameHash[1], nameHash[2], nameHash[3], nameHash[4], nameHash[5], nameHash[6], nameHash[7]], 0) / (float)ulong.MaxValue;
                return NitroxColor.FromHsb(hue);
            }
        }

        return new NitroxColor(1, 1, 1);
    }
}
