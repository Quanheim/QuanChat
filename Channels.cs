using System;
using HarmonyLib;
using TMPro;

namespace QuanChat
{
    // Sticky channels, WoW style. Untagged messages go to the current channel (shout unless switched). "/s" or
    // "/say" on their own switch it; with text after them they send one message and leave the channel alone
    // (vanilla already does that part). The channel resets to the Starting channel setting at every login.
    internal static class Channels
    {
        internal static Channel Current = Channel.Shout;

        // Chat is created with each world session, so this is "every login".
        [HarmonyPatch(typeof(Chat), nameof(Chat.Awake))]
        private static class ResetOnLogin
        {
            private static void Postfix() => Current = Settings.StartingChannel.Value;
        }

        // Vanilla Chat.InputText: text starting with '/' runs as a command, anything else as "say <text>".
        [HarmonyPatch(typeof(Chat), "InputText")]
        private static class RouteInput
        {
            private static bool Prefix(Chat __instance)
            {
                string text = __instance.m_input.text;
                string trimmed = text.Trim();

                if (trimmed.Equals("/s", StringComparison.OrdinalIgnoreCase))
                    return Switch(__instance, Channel.Shout);
                if (trimmed.Equals("/say", StringComparison.OrdinalIgnoreCase))
                    return Switch(__instance, Channel.Say);

                if (text.Length > 0 && text[0] != '/' && Current == Channel.Shout)
                {
                    __instance.TryRunCommand("s " + text);
                    return false;
                }
                return true; // a command, or local talk: vanilla's own path
            }

            private static bool Switch(Chat chat, Channel channel)
            {
                Current = channel;
                chat.AddString(channel == Channel.Shout
                    ? "<color=#B0B0B0>Channel: Shout. Your messages reach everyone. /say to talk nearby.</color>"
                    : "<color=#B0B0B0>Channel: Say. Your messages reach players within 15 m. /s to shout.</color>");
                return false;
            }
        }

        // The channel indicator: the empty input's placeholder reads "[Shout] ..." or "[Say] ...".
        private static string _placeholderOriginal;
        private static string _placeholderSet;

        internal static void UpdatePlaceholder(Chat chat)
        {
            if (!(chat.m_input.placeholder is TMP_Text placeholder))
                return;

            string current = placeholder.text;
            if (current != _placeholderSet)
                _placeholderOriginal = current; // vanilla (or a language change) wrote its own text

            string wanted = "[" + Current + "] " +
                            (string.IsNullOrEmpty(_placeholderOriginal) ? "..." : _placeholderOriginal);
            if (current != wanted)
            {
                placeholder.text = wanted;
                _placeholderSet = wanted;
            }
        }
    }
}
