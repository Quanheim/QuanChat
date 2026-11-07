using System.Reflection;
using HarmonyLib;

namespace QuanChat
{
    // Hanging indent and the gap between messages. Terminal.UpdateChat rebuilds the chat window's text from the
    // buffer, one message per line; this adds TextMeshPro layout on top of that, so the buffer itself is untouched
    // and changing a setting re-lays out the history too.
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

            if (_vanillaParagraphSpacing == null)
                _vanillaParagraphSpacing = __instance.m_output.paragraphSpacing;
            __instance.m_output.paragraphSpacing =
                Settings.SpaceBetweenMessages.Value ? MessageGap : _vanillaParagraphSpacing.Value;

            if (Settings.HangingIndent.Value)
                __instance.m_output.text = HangingIndentTags + __instance.m_output.text;
        }

        internal static void Refresh()
        {
            if (Chat.instance != null)
                UpdateChatMethod.Invoke(Chat.instance, null);
        }
    }
}
