using System.IO;
using BepInEx;
using BepInEx.Configuration;
using Jotunn.Managers;

namespace QuanChat
{
    // TEST BUILD ONLY (branch test/server-config): a second config file for server-enforced settings, kept apart
    // from the personal quandru.QuanChat.cfg so that one can be excluded from the Gale export. One dummy setting
    // checks that the file is written on the server, whether F1 shows it, and that Jotunn enforces the server's
    // value on clients.
    internal static class ServerSettingsTest
    {
        internal static ConfigEntry<string> TestValue;

        internal static void Bind(BepInPlugin info)
        {
            var file = new ConfigFile(Path.Combine(Paths.ConfigPath, Plugin.PluginGuid + ".Server.cfg"), true, info);
            TestValue = file.Bind("0 - Server (enforced)", "Test value", "default",
                new ConfigDescription("Test only. The server's value should replace yours when you connect.", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
            SynchronizationManager.Instance.RegisterCustomConfig(file);

            Plugin.Log.LogInfo($"QuanChat test value (local file): {TestValue.Value}");
            TestValue.SettingChanged += (_, __) => Plugin.Log.LogInfo($"QuanChat test value changed: {TestValue.Value}");
            SynchronizationManager.OnConfigurationSynchronized += (_, args) =>
                Plugin.Log.LogInfo($"QuanChat test value: {TestValue.Value} (after " +
                                   $"{(args.InitialSynchronization ? "initial" : "later")} sync from the server)");
        }
    }
}
