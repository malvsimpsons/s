using System.IO;
using System.Runtime.InteropServices;
using Nitrox.Server.Subnautica.Models.Configuration;
using NitroxModel.Helper;
using NitroxModel.Server;

namespace Nitrox.Server.Subnautica.Extensions;

public static class SubnauticaServerOptionsExtensions
{
    public static bool IsHardcore(this SubnauticaServerOptions options) => options.GameMode == NitroxGameMode.HARDCORE;
    public static bool IsPasswordRequired(this SubnauticaServerOptions options) => options.ServerPassword != "";
}
