using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuanChat
{
    // The chat window's size, position, background and font size.
    //
    // Vanilla's window (Chat_box, from the main scene) is stored as 500 x 400, anchored bottom right, 67 units
    // above the bottom edge. It has its own Canvas, so at runtime it covers the screen and those values are only
    // the base for our Width, Height and gap. The visible box is its child "root", which holds "bkg" (a black
    // image whose sprite is a gradient), "Text" (the output, 18 pt) and "ChatInput" (20 pt, a fixed 500 wide).
    // The stored child positions don't match what's drawn, so this sets every rect explicitly: root placed from
    // the screen's bottom-right corner, the background filling it, the input a strip along its bottom, and the
    // output filling the space above.
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
            Apply(__instance, log: false);
            _laidOutFor = default; // lay out and log again on the first frame, when the window has its real size
            _loggedSettled = false;
        }

        internal static void Refresh()
        {
            if (Chat.instance != null && _vanillaFontSize > 0f)
                Apply(Chat.instance, log: false);
        }

        // Called every frame from ChatWindow: lays out again once the window has its real size (it may
        // not at login) and whenever the resolution changes.
        private static Vector2Int _laidOutFor;
        private static bool _loggedSettled;

        internal static void KeepInPlace(Chat chat)
        {
            if (_vanillaFontSize <= 0f || (_laidOutFor.x == Screen.width && _laidOutFor.y == Screen.height))
                return;
            Apply(chat, log: !_loggedSettled);
            _loggedSettled = true;
        }

        private static void Apply(Chat chat, bool log)
        {
            float fontScale = Settings.FontSize.Value / _vanillaFontSize;
            float inputHeight = _vanillaInputHeight * fontScale;
            float edge = _vanillaBottom / 2f; // half vanilla's gap below, and the same gap on the right
            Vector2 size = new Vector2(_vanillaSize.x * Settings.Width.Value / 100f,
                                       _vanillaSize.y * Settings.Height.Value / 100f);

            // Chat_box carries its own Canvas, which Unity stretches to the whole screen, so its own size and
            // position are ignored (0.2.0 filled the screen width). The visible box is "root": anchor it to
            // Chat_box's bottom-right corner, `edge` in from each side, and Unity keeps it there at any resolution.
            // (0.2.1 measured the screen canvas instead, but GetComponentInParent skips the hidden window.)
            RectTransform window = chat.m_chatWindow;
            RectTransform root = window.Find("root") as RectTransform;
            if (root != null)
            {
                root.anchorMin = root.anchorMax = root.pivot = new Vector2(1f, 0f);
                root.anchoredPosition = new Vector2(-edge, edge);
                root.sizeDelta = size;
                if (root.GetComponent<RectMask2D>() == null)
                    root.gameObject.AddComponent<RectMask2D>(); // backstop: nothing draws outside the box
            }
            _laidOutFor = new Vector2Int(Screen.width, Screen.height);

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
                Plugin.Log.LogInfo($"Chat window laid out on a {Screen.width} x {Screen.height} screen: box " +
                                   $"{size.x:0} x {size.y:0}, {edge:0} in from the bottom-right corner, text area " +
                                   $"{chat.m_output.rectTransform.rect.width:0} x " +
                                   $"{chat.m_output.rectTransform.rect.height:0}, input {inputHeight:0} high, " +
                                   $"root {(root != null ? "found" : "MISSING")}, window (should match the screen in canvas units) " +
                                   $"{window.rect.width:0} x {window.rect.height:0}, " +
                                   $"background {(bkg != null ? "made even" : "MISSING")}.");
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
