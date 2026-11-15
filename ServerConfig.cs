using System;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using Jotunn.Managers;
using UnityEngine;

namespace QuanChat
{
    // Server-enforced settings live in their own file, quandru.QuanChat.Server.cfg, so the personal
    // quandru.QuanChat.cfg can stay excluded from the Gale profile's export while this one is synced.
    //
    // How it behaves (checked against Jotunn 2.30.2's SynchronizationManager, 15 Nov 2026):
    // - Jotunn sends the server's values to every client on connect and uses them instead of the client's own. It
    //   holds them in memory only, so a client's file (and Gale's editor) keep showing the local value.
    // - The Configuration Manager (F1) doesn't list a second config file, so admins change these by editing the
    //   server's file, by hand or in Gale's config editor on the server.
    // - This class watches that file. After an edit the server reloads it, and Jotunn (which listens for
    //   ConfigReloaded on every file it syncs) sends the changed values to every connected player straight away.
    // - While connected to a server, a client ignores file values for synced settings (Jotunn blocks them), so an
    //   edit on a client only takes effect offline or when that player hosts.
    internal static class ServerConfig
    {
        internal const string FileName = Plugin.PluginGuid + ".Server.cfg";

        internal static ConfigFile File { get; private set; }

        // Gale and editors often write a file in several steps, so wait for a quiet second before reloading.
        private const float QuietSeconds = 1f;
        private const int MaxReadRetries = 5;

        private static FileSystemWatcher watcher;
        private static volatile bool fileEvent;
        private static float reloadAt = -1f;
        private static int readRetries;
        private static string lastText;

        // Before binding any setting.
        internal static void Create(BepInPlugin info)
        {
            File = new ConfigFile(Path.Combine(Paths.ConfigPath, FileName), true, info);
        }

        // After binding every setting: hand the file to Jotunn and start watching it.
        internal static void Register()
        {
            SynchronizationManager.Instance.RegisterCustomConfig(File);
            lastText = ReadText();

            try
            {
                watcher = new FileSystemWatcher(Paths.ConfigPath, FileName)
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName |
                                   NotifyFilters.CreationTime,
                };
                watcher.Changed += OnFileEvent;
                watcher.Created += OnFileEvent;
                watcher.Renamed += OnFileEvent;
                watcher.EnableRaisingEvents = true;
                Plugin.Log.LogInfo($"Watching {FileName} for edits. On a server, an edit reaches every connected " +
                                   "player without a restart.");
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning($"Can't watch {FileName} for edits ({e.Message}). Server settings still " +
                                      "work, but a change to the file needs a server restart.");
            }
        }

        // Runs on a background thread: only flag it, and reload from Update on the main thread.
        private static void OnFileEvent(object sender, FileSystemEventArgs e) => fileEvent = true;

        // Called from Plugin.Update.
        internal static void Update()
        {
            if (fileEvent)
            {
                fileEvent = false;
                reloadAt = Time.realtimeSinceStartup + QuietSeconds;
                readRetries = 0;
            }

            if (reloadAt < 0f || Time.realtimeSinceStartup < reloadAt) return;
            reloadAt = -1f;
            ReloadIfEdited();
        }

        private static void ReloadIfEdited()
        {
            string text = ReadText();
            if (text == null)
            {
                // Still being written, or locked by the editor: try again shortly.
                if (++readRetries <= MaxReadRetries) reloadAt = Time.realtimeSinceStartup + QuietSeconds;
                else Plugin.Log.LogWarning($"{FileName} changed but couldn't be read after {MaxReadRetries} tries. " +
                                           "Save it again to retry.");
                return;
            }

            // Our own save (BepInEx rewrites the file when a value is set) or a save with no real change.
            if (text == lastText) return;
            lastText = text;

            if (ZNet.instance != null && !ZNet.instance.IsServer())
            {
                Plugin.Log.LogInfo($"{FileName} was edited, but you're connected to a server, which sets these " +
                                   "values. Your edit takes effect offline or when you host. To change them for " +
                                   "everyone, edit the server's copy.");
                return;
            }

            File.Reload();
            lastText = ReadText() ?? lastText;
            bool online = ZNet.instance != null && ZNet.instance.IsServer() && ZNet.instance.GetPeers().Count > 0;
            Plugin.Log.LogInfo($"{FileName} was edited and has been reloaded." +
                               (online ? " Changed values are being sent to every connected player now." : ""));
        }

        private static string ReadText()
        {
            try
            {
                return System.IO.File.ReadAllText(File.ConfigFilePath);
            }
            catch (IOException)
            {
                return null;
            }
            catch (UnauthorizedAccessException)
            {
                return null;
            }
        }
    }
}
