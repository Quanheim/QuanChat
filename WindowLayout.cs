using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuanChat
{
    // The chat window's size, position, background and font size.
    //
    // Vanilla's window (Chat_box, from the main scene) is 500 x 400, anchored bottom right, 67 units above the
    // bottom edge and flush with the right. It holds "root", which holds "bkg" (a black image whose sprite is a
    // gradient), "Text" (the output, 18 pt) and "ChatInput" (20 pt, a fixed 500 wide). Its stored child positions
    // don't match what's on screen, so rather than adjust them this sets every rect explicitly: the background
    // fills the window, the input is a strip along the bottom, and the output fills the space above it.
    [HarmonyPatch(typeof(Chat), nameof(Chat.Awake))]
    internal static class WindowLayout
    {
        // Vanilla shows at most 20 messages; we show as many as fit the window (MessageLayout trims to fit).
        private const int MaxMessages = 100;
        private const float TextPadding = 6f;

        private static Vector2 _vanillaSize;
        private static float _vanillaBottom;
        private static float _vanillaFontSize;
        private static float _vanillaInputHeight;
        private static float _vanillaInputFontSize;

        private static void Postfix(Chat __instance)
        {
            RectTransform window = __instance.m_chatWindow;
            if (window == null || __instance.m_output == null || __instance.m_input == null)
                return;

            // Chat is created fresh each login, so these are always the prefab's own values.
            _vanillaSize = window.sizeDelta;
            _vanillaBottom = window.anchoredPosition.y;
            _vanillaFontSize = __instance.m_output.fontSize;
            _vanillaInputHeight = ((RectTransform)__instance.m_input.transform).sizeDelta.y;
            _vanillaInputFontSize = __instance.m_input.pointSize;

            __instance.m_maxVisibleBufferLength = MaxMessages;
            Apply(__instance, log: true);
        }

        internal static void Refresh()
        {
            if (Chat.instance != null && _vanillaFontSize > 0f)
                Apply(Chat.instance, log: false);
        }

        private static void Apply(Chat chat, bool log)
        {
            float fontScale = Settings.FontSize.Value / _vanillaFontSize;
            float inputHeight = _vanillaInputHeight * fontScale;
            float edge = _vanillaBottom / 2f; // half vanilla's gap below, and the same gap on the right
            Vector2 size = new Vector2(_vanillaSize.x * Settings.Width.Value / 100f,
                                       _vanillaSize.y * Settings.Height.Value / 100f);

            RectTransform window = chat.m_chatWindow;
            window.anchorMin = window.anchorMax = window.pivot = new Vector2(1f, 0f);
            window.anchoredPosition = new Vector2(-edge, edge);
            window.sizeDelta = size;

            RectTransform root = window.Find("root") as RectTransform;
            if (root != null)
            {
                Fill(root);
                if (root.GetComponent<RectMask2D>() == null)
                    root.gameObject.AddComponent<RectMask2D>(); // backstop: nothing draws outside the window
            }

            // One even shade over the whole window instead of vanilla's gradient: same colour and opacity.
            Image bkg = (root != null ? root : window).Find("bkg")?.GetComponent<Image>();
            if (bkg != null)
            {
                Fill(bkg.rectTransform);
                bkg.sprite = null;
            }

            RectTransform input = (RectTransform)chat.m_input.transform;
            input.anchorMin = Vector2.zero;
            input.anchorMax = new Vector2(1f, 0f);
            input.pivot = new Vector2(0.5f, 0f);
            input.anchoredPosition = Vector2.zero;
            input.sizeDelta = new Vector2(0f, inputHeight);
            chat.m_input.pointSize = _vanillaInputFontSize * fontScale;

            TextMeshProUGUI output = chat.m_output;
            RectTransform text = output.rectTransform;
            text.anchorMin = Vector2.zero;
            text.anchorMax = Vector2.one;
            text.pivot = new Vector2(0.5f, 0.5f);
            text.offsetMin = new Vector2(TextPadding, inputHeight);
            text.offsetMax = new Vector2(-TextPadding, -TextPadding);
            output.margin = Vector4.zero;
            output.fontSize = Settings.FontSize.Value;

            MessageLayout.Refresh();

            if (log)
                Plugin.Log.LogInfo($"Chat window laid out: window {size.x:0} x {size.y:0}, text area " +
                                   $"{text.rect.width:0} x {text.rect.height:0}, input {inputHeight:0} high, " +
                                   $"root {(root != null ? "found" : "MISSING")}, background " +
                                   $"{(bkg != null ? "made even" : "MISSING")}.");
        }

        private static void Fill(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
