using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace QuanChat
{
    // Messages appear as typed. Vanilla forces shouts to capitals and whispers to lowercase in two places: the chat
    // window line (Terminal.AddString with a sender) and the floating text over the speaker (Chat.AddInworldText).
    // The transpiler swaps those string.ToUpper / ToLowerInvariant calls for a pass-through, keeping the colours.
    [HarmonyPatch]
    internal static class MessagesAsTyped
    {
        internal static MethodBase AddStringFromPlayer() =>
            AccessTools.GetDeclaredMethods(typeof(Terminal))
                .First(m => m.Name == nameof(Terminal.AddString) && m.GetParameters().Length == 4);

        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AddStringFromPlayer();
            yield return AccessTools.Method(typeof(Chat), "AddInworldText");
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
            MethodBase original)
        {
            MethodInfo toUpper = AccessTools.Method(typeof(string), nameof(string.ToUpper), Type.EmptyTypes);
            MethodInfo toLower = AccessTools.Method(typeof(string), nameof(string.ToLowerInvariant), Type.EmptyTypes);
            MethodInfo keep = AccessTools.Method(typeof(MessagesAsTyped), nameof(Keep));

            int replaced = 0;
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.Calls(toUpper) || instruction.Calls(toLower))
                {
                    var call = new CodeInstruction(OpCodes.Call, keep);
                    call.labels.AddRange(instruction.labels);
                    call.blocks.AddRange(instruction.blocks);
                    replaced++;
                    yield return call;
                }
                else
                {
                    yield return instruction;
                }
            }

            if (replaced != 2)
                Plugin.Log.LogWarning($"Messages as typed: expected 2 forced-case calls in {original.DeclaringType?.Name}." +
                                      $"{original.Name}, found {replaced}. A game update may have changed it.");
        }

        private static string Keep(string text) => text;
    }

    // A new message from a player jumps the window back to the newest line (vanilla leaves it scrolled).
    [HarmonyPatch]
    internal static class ScrollToNewest
    {
        private static MethodBase TargetMethod() => MessagesAsTyped.AddStringFromPlayer();

        private static void Prefix(Terminal __instance)
        {
            if (__instance is Chat)
                ChatWindow.ScrollToBottom(__instance);
        }
    }

    // Shout map pings: Minimap.UpdateShoutPins draws a pin for each shout Chat.GetShoutWorldTexts returns.
    // Returning none (the caller clears the list first) means no pins, and any showing are removed.
    [HarmonyPatch(typeof(Chat), nameof(Chat.GetShoutWorldTexts))]
    internal static class HideShoutMapPings
    {
        private static bool Prefix() => !Settings.HideShoutMapPings.Value;
    }
}
