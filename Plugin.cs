using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Jotunn.Utils;
using UnityEngine;
using UnityEngine.Rendering;

namespace QuanChat
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    // v1 is client-side only, but the server carries it too so everyone's version matches (and private messages
    // and talk distance, planned, will need it there).
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "quandru.QuanChat";
        public const string PluginName = "QuanChat";
        public const string PluginVersion = PluginInfo.Version; // generated from <Version> in the .csproj

        internal static ManualLogSource Log;

        private void Awake()
        {
            Log = Logger;

            // A dedicated server has no chat window, so there's nothing for QuanChat to do there. Skip the patches
            // and the config file: every setting is personal, and a server copy would only travel to players in a
            // Gale profile sync.
            if (IsDedicatedServer())
            {
                Logger.LogInfo($"{PluginName} {PluginVersion} loaded on a dedicated server: its features are all " +
                               "client-side, so they're disabled here and no config file is written. It's installed " +
                               "on the server so every player's version matches.");
                return;
            }

            Settings.Bind(Config);
            new Harmony(PluginGuid).PatchAll(typeof(Plugin).Assembly);
            Logger.LogInfo($"{PluginName} {PluginVersion} loaded.");
        }

        // Valheim's dedicated server runs headless: no graphics device, batch mode.
        private static bool IsDedicatedServer() =>
            SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null || Application.isBatchMode;
    }
}
