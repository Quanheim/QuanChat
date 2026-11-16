using BepInEx.Configuration;
using Jotunn.Managers;

namespace QuanChat
{
    // Scaffolding for server-enforced settings (1.0.2): one placeholder setting that does nothing but log, in
    // QuanChat's own config file, in a section after the personal ones (sections are never renamed after release).
    // The first real enforced setting goes in the same section and replaces it. Jotunn syncs it because it's
    // admin-only: every player gets the server's value on connect, admins change it live in F1 (Jotunn sends the
    // change to the server, which saves it and passes it on), and non-admins see it locked. It doesn't need Gale:
    // the server's own copy is the source, even though the file is excluded from the profile's export.
    internal static class ServerSettingsTest
    {
        internal static ConfigEntry<string> TestValue;

        internal static void Bind(ConfigFile config)
        {
            TestValue = config.Bind("3 - Server (admin only)", "Test value", "default",
                new ConfigDescription("Placeholder for testing server settings; changes nothing in game. Synced from the " +
                                      "server; admins only.", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

            Plugin.Log.LogInfo($"QuanChat test value (local file): {TestValue.Value}");
            TestValue.SettingChanged += (_, __) => Plugin.Log.LogInfo($"QuanChat test value changed: {TestValue.Value}");
            SynchronizationManager.OnConfigurationSynchronized += (_, args) =>
                Plugin.Log.LogInfo($"QuanChat test value: {TestValue.Value} (after " +
                                   $"{(args.InitialSynchronization ? "initial" : "later")} sync from the server)");
        }
    }
}
