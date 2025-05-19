#if DEBUG
using System;
using System.Net;
using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.MultiplayerSession;
using NitroxClient.MonoBehaviours.Gui.MainMenu.ServerJoin;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Networking.Session;
using NitroxModel.Server;

namespace NitroxPatcher.Patches.Persistent;

// TODO: Rework this to be less ad hoc and more robust with command line arguments
public sealed partial class uGUI_MainMenu_Start_Patch : NitroxPatch, IPersistentPatch
{
    private static readonly MethodInfo TARGET_METHOD = AccessTools.EnumeratorMoveNext(Reflect.Method((uGUI_MainMenu t) => t.Start()));

    private static bool applied;
    private static string playerName;

    public static void Postfix()
    {
        if (applied)
        {
            return;
        }
        applied = true;

        string[] args = Environment.GetCommandLineArgs();
        Log.Info($"CommandLineArgs: {string.Join(" ", args)}");
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].Equals("--instantlaunch", StringComparison.OrdinalIgnoreCase) && args.Length > i + 1)
            {
                playerName = args[i + 1];
                Log.Info($"Detected instant launch, connecting to 127.0.0.1:{ServerConstants.DEFAULT_PORT} as {playerName}");
                _ = JoinServerBackend.StartDetachedMultiplayerClientAsync(IPAddress.Loopback, ServerConstants.DEFAULT_PORT, SessionConnectionStateChangedHandler);
            }
        }
    }

    private static void SessionConnectionStateChangedHandler(IMultiplayerSessionConnectionState state)
    {
        switch (state.CurrentStage)
        {
            case MultiplayerSessionConnectionStage.AWAITING_RESERVATION_CREDENTIALS:

                if (Resolve<IMultiplayerSession>().SessionPolicy.RequiresServerPassword)
                {
                    Log.Error("Local server requires a password which is not supported with instant launch.");
                    Log.InGame("Local server requires a password which is not supported with instant launch.");
                    break;
                }

                PlayerSettings playerSettings = new(playerName, PlayerNameHelper.GenerateColorByName(playerName));
                AuthenticationContext authenticationContext = new(Optional.Empty, Optional.Empty);
                Resolve<IMultiplayerSession>().RequestSessionReservation(playerSettings, authenticationContext);
                break;

            case MultiplayerSessionConnectionStage.SESSION_RESERVED:
                Resolve<IMultiplayerSession>().ConnectionStateChanged -= SessionConnectionStateChangedHandler;
                JoinServerBackend.StartGame();
                break;
        }
    }
}
#endif
