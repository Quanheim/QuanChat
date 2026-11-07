using System.Reflection;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace QuanChat
{
    // Hanging indent, the gap between messages, and fitting the messages to the window. Terminal.UpdateChat
    // rebuilds the chat window's text from the buffer, one message per line; this adds TextMeshPro layout on top
    // of that, so the buffer itself is untouched and changing a setting re-lays out the history too.
    [HarmonyPatch(typeof(Terminal), "UpdateChat")]
    internal static class MessageLayout
    {
        // <indent> moves every line, wrapped ones included; <line-indent> applies only after a manual line break
        // (a new message), so the negative value brings each message's first line back to the margin.
        private const string HangingIndentTags = "<indent=2em><line-indent=-2em>";

        // TextMeshPro paragraph spacing, in hundredths of an em, added after each message's line break.
        private const float MessageGap = 60f;

        private static readonly MethodInfo UpdateChatMethod = AccessTools.Method(typeof(Terminal), "UpdateChat");
        private static float? _vanillaParagraphSpacing;

        private static void Postfix(Terminal __instance)
        {
            if (!(__instance is Chat) || __instance.m_output == null)
                return;
            TextMeshProUGUI output = __instance.m_output;

            if (_vanillaParagraphSpacing == null)
                _vanillaParagraphSpacing = output.paragraphSpacing;
            output.paragraphSpacing = Settings.SpaceBetweenMessages.Value ? MessageGap : _vanillaParagraphSpacing.Value;

            string prefix = Settings.HangingIndent.Value ? HangingIndentTags : "";
            string[] lines = output.text.TrimEnd('\n').Split('\n');
            output.text = prefix + FitToWindow(output, prefix, lines);
        }

        // Drops the oldest messages until the rest fit the text area, so nothing spills above the window. The
        // newest message always shows. Binary search: a handful of layouts per redraw, however long the history.
        private static string FitToWindow(TextMeshProUGUI output, string prefix, string[] lines)
        {
            Rect area = output.rectTransform.rect;
            if (area.width <= 0f || area.height <= 0f)
                return string.Join("\n", lines);

            int first = 0, last = lines.Length - 1;
            while (first < last)
            {
                int mid = (first + last) / 2;
                string candidate = prefix + string.Join("\n", lines, mid, lines.Length - mid);
                if (output.GetPreferredValues(candidate, area.width, 0f).y <= area.height)
                    last = mid;
                else
                    first = mid + 1;
            }
            return string.Join("\n", lines, first, lines.Length - first);
        }

        internal static void Refresh()
        {
            if (Chat.instance != null)
                UpdateChatMethod.Invoke(Chat.instance, null);
        }
    }
}
