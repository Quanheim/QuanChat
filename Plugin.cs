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

            // The config file is written on a dedicated server too, though nothing there reads it. Every setting is
            // personal, so the server's copy must be excluded from the Gale profile's export; Gale can only exclude a
            // file that exists, and without the exclusion each sync deletes every player's own copy.
            Settings.Bind(Config);

            // A dedicated server has no chat window, so there's nothing else for QuanChat to do there.
            if (IsDedicatedServer())
            {
                Logger.LogInfo($"{PluginName} {PluginVersion} loaded on a dedicated server: its features are all " +
                               "client-side, so they're disabled here. Its config file is written so it can be " +
                               "excluded from the Gale profile's export. It's installed on the server so every " +
                               "player's version matches.");
                return;
            }

            new Harmony(PluginGuid).PatchAll(typeof(Plugin).Assembly);
            Logger.LogInfo($"{PluginName} {PluginVersion} loaded.");
        }

        // Valheim's dedicated server runs headless: no graphics device, batch mode.
        private static bool IsDedicatedServer() =>
            SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null || Application.isBatchMode;
    }
}
