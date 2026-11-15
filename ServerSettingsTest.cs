using BepInEx.Configuration;
using Jotunn.Managers;

namespace QuanChat
{
    // TEST BUILD ONLY (branch test/server-config): one dummy setting in the server-enforced file (ServerConfig),
    // to check the file is written on the server, that Jotunn enforces the server's value on clients, and that an
    // edit to the server's file reaches connected players without a restart.
    internal static class ServerSettingsTest
    {
        internal static ConfigEntry<string> TestValue;

        internal static void Bind()
        {
            TestValue = ServerConfig.File.Bind("0 - Server (enforced)", "Test value", "default",
                new ConfigDescription("Test only. The server's value should replace yours when you connect.", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

            Plugin.Log.LogInfo($"QuanChat test value (local file): {TestValue.Value}");
            TestValue.SettingChanged += (_, __) => Plugin.Log.LogInfo($"QuanChat test value changed: {TestValue.Value}");
            SynchronizationManager.OnConfigurationSynchronized += (_, args) =>
                Plugin.Log.LogInfo($"QuanChat test value: {TestValue.Value} (after " +
                                   $"{(args.InitialSynchronization ? "initial" : "later")} sync from the server)");
        }
    }
}
