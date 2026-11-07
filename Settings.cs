using BepInEx.Configuration;

namespace QuanChat
{
    internal enum Channel
    {
        Shout,
        Say
    }

    // Every v1 setting is personal: nothing here affects other players, so nothing is synced from the server.
    internal static class Settings
    {
        internal static ConfigEntry<Channel> StartingChannel;

        internal static ConfigEntry<bool> AlwaysShow;
        internal static ConfigEntry<int> SecondsUntilIdle;
        internal static ConfigEntry<int> IdleOpacity;
        internal static ConfigEntry<bool> ClickThrough;
        internal static ConfigEntry<int> Width;
        internal static ConfigEntry<int> Height;

        internal static ConfigEntry<bool> HangingIndent;
        internal static ConfigEntry<bool> SpaceBetweenMessages;
        internal static ConfigEntry<bool> HideShoutMapPings;
        internal static ConfigEntry<int> FontSize;

        internal static void Bind(ConfigFile config)
        {
            const string general = "0 - General (personal)";
            StartingChannel = config.Bind(general, "Starting channel", Channel.Shout,
                "The channel you start in each time you log in. Shout reaches everyone on the server; Say reaches " +
                "players within 15 m. Switch during play by typing /s or /say on its own. Personal.");

            const string window = "1 - Window (personal)";
            AlwaysShow = config.Bind(window, "Always show", false,
                "Off: the chat window fades out entirely once it's idle, and comes back with the next message. " +
                "On: it stays on screen at the idle opacity. Personal.");
            SecondsUntilIdle = config.Bind(window, "Seconds until idle", 30, new ConfigDescription(
                "How long the chat window stays at full opacity after the newest message before it fades. Personal.",
                new AcceptableValueRange<int>(5, 300)));
            IdleOpacity = config.Bind(window, "Idle opacity", 50, new ConfigDescription(
                "How visible the chat window stays once idle, in percent, when Always show is on. Personal.",
                new AcceptableValueRange<int>(10, 100)));
            ClickThrough = config.Bind(window, "Click-through", true,
                "The chat window ignores mouse clicks while you aren't typing, so it never gets in the way of what's " +
                "behind it. Personal.");
            Width = config.Bind(window, "Width", 100, new ConfigDescription(
                "Width of the chat window, in percent of the game's own. It stays anchored at the bottom right. " +
                "Personal.", new AcceptableValueRange<int>(50, 300)));
            Height = config.Bind(window, "Height", 100, new ConfigDescription(
                "Height of the chat window, in percent of the game's own. It grows upwards, and shows as many " +
                "messages as fit. Personal.", new AcceptableValueRange<int>(50, 300)));

            const string messages = "2 - Messages (personal)";
            HangingIndent = config.Bind(messages, "Hanging indent", true,
                "Indent the wrapped lines of each message, so every new message visibly starts at the margin. " +
                "Personal.");
            SpaceBetweenMessages = config.Bind(messages, "Space between messages", true,
                "Leave a small gap, about half a line, between messages. Personal.");
            HideShoutMapPings = config.Bind(messages, "Hide shout map pings", true,
                "Shouts don't flash a pin on your map. The text over a nearby speaker's head still shows. Personal.");
            FontSize = config.Bind(messages, "Font size", 18, new ConfigDescription(
                "Size of the chat text, in points (the game's own is 18). The input box grows with it. Personal.",
                new AcceptableValueRange<int>(10, 40)));

            HangingIndent.SettingChanged += (_, __) => MessageLayout.Refresh();
            SpaceBetweenMessages.SettingChanged += (_, __) => MessageLayout.Refresh();
            Width.SettingChanged += (_, __) => WindowLayout.Refresh();
            Height.SettingChanged += (_, __) => WindowLayout.Refresh();
            FontSize.SettingChanged += (_, __) => WindowLayout.Refresh();
        }
    }
}
