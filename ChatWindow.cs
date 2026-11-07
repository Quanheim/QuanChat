using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace QuanChat
{
    // The chat window's visibility, fade, click-through and scrolling.
    //
    // Vanilla shows the window while m_hideTimer < m_hideDelay (10 s) and hides it outright after; the timer
    // resets on every new message and while typing. This runs after vanilla's Chat.Update each frame and takes
    // over: full opacity for "Seconds until idle", then a one-second fade, to nothing or to the idle opacity.
    [HarmonyPatch(typeof(Chat), "Update")]
    internal static class ChatWindow
    {
        private const float FadeSeconds = 1f;

        private static readonly AccessTools.FieldRef<Chat, float> HideTimer =
            AccessTools.FieldRefAccess<Chat, float>("m_hideTimer");
        private static readonly AccessTools.FieldRef<Terminal, int> ScrollHeight =
            AccessTools.FieldRefAccess<Terminal, int>("m_scrollHeight");
        private static readonly AccessTools.FieldRef<Terminal, List<string>> ChatBuffer =
            AccessTools.FieldRefAccess<Terminal, List<string>>("m_chatBuffer");
        private static readonly MethodInfo UpdateChat = AccessTools.Method(typeof(Terminal), "UpdateChat");

        private static void Postfix(Chat __instance)
        {
            RectTransform window = __instance.m_chatWindow;
            if (window == null)
                return;

            // Vanilla shows the input while you're typing and hides it when you send or close.
            bool typing = __instance.m_input != null && __instance.m_input.gameObject.activeSelf;
            if (typing)
                Channels.UpdatePlaceholder(__instance);

            bool hidden = IsHidden(typing);
            if (!hidden && !typing)
                ScrollWithoutTyping(__instance);

            float alpha = typing ? 1f : IdleAlpha(HideTimer(__instance));
            bool show = !hidden && alpha > 0.001f;
            if (window.gameObject.activeSelf != show)
                window.gameObject.SetActive(show);
            if (!show)
                return;

            CanvasGroup group = window.GetComponent<CanvasGroup>() ?? window.gameObject.AddComponent<CanvasGroup>();
            group.alpha = alpha;
            group.blocksRaycasts = typing || !Settings.ClickThrough.Value;
        }

        private static float IdleAlpha(float secondsSinceLastMessage)
        {
            float idleAfter = Settings.SecondsUntilIdle.Value;
            if (secondsSinceLastMessage <= idleAfter)
                return 1f;
            float target = Settings.AlwaysShow.Value ? Settings.IdleOpacity.Value / 100f : 0f;
            float fade = Mathf.Clamp01((secondsSinceLastMessage - idleAfter) / FadeSeconds);
            return Mathf.Lerp(1f, target, fade);
        }

        // Hidden with the HUD (Ctrl+F3) and in cutscenes, during the intro, in the Esc menu, and under the big
        // map. While typing it stays up unless the menu is open, so you can always see what you're writing.
        private static bool IsHidden(bool typing)
        {
            Player player = Player.m_localPlayer;
            if (player == null || player.InIntro() || Menu.IsVisible())
                return true;
            if (typing)
                return false;
            if (Hud.instance != null && Hud.instance.m_userHidden)
                return true;
            return player.InCutscene() || Minimap.IsOpen();
        }

        // Vanilla only reads Page Up / Page Down while you're typing (Terminal.UpdateInput).
        private static void ScrollWithoutTyping(Chat chat)
        {
            if (Console.IsVisible() || TextInput.IsVisible() || Minimap.InTextInput())
                return;

            int delta = 0;
            if (ZInput.GetButtonDown("ScrollChatUp") || ZInput.GetButtonDown("JoyScrollChatUp"))
                delta = 1;
            else if (ZInput.GetButtonDown("ScrollChatDown") || ZInput.GetButtonDown("JoyScrollChatDown"))
                delta = -1;
            if (delta == 0)
                return;

            ref int scroll = ref ScrollHeight(chat);
            int max = Mathf.Max(0, ChatBuffer(chat).Count - 5); // vanilla's limit
            int next = Mathf.Clamp(scroll + delta, 0, max);
            HideTimer(chat) = 0f; // scrolling brings the window back to full opacity
            if (next == scroll)
                return;
            scroll = next;
            UpdateChat.Invoke(chat, null);
        }

        // A new chat message jumps back to the newest line.
        internal static void ScrollToBottom(Terminal terminal) => ScrollHeight(terminal) = 0;
    }
}
