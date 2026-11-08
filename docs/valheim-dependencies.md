# Valheim Code Dependency

Everything this mod relies on from Valheim, so a game update can be checked quickly. `check-mods.ps1` in the
`.decompiles` repo reads the **Game code** tables and flags any entry the game changed. See "Valheim updates" in
`docs/DEVELOPMENT.md` for the whole check.

**Checked against Valheim:** 1.0.21 (2026-11-02)

Update this file in the same commit as any code that patches, calls or relies on something new in the game, and
update the version line above each time the mod passes the Valheim update check.

## Game code

The first column holds the game class or member in backticks, as `Class` or `Class.Member`. Nested types use their
parent's name, e.g. `Player.PlacementStatus`. Only the first column is read by the script; the rest is for people.

### Harmony patches

What the mod patches. A changed signature stops the patch applying; changed logic can make it wrong.

| Game code | Patch | What it does | If the game changes it |
|---|---|---|---|
| `Chat.Awake` | Postfix (two) | Resets the channel to the Starting channel setting (Chat is created each login). Lays out the window: size, position, even background, font size; raises the message cap. | Channel no longer resets per login; window back to vanilla layout. |
| `Chat.InputText` | Prefix | `/s`, `/shout` or `/say` alone switch channel; `/shout <text>` sends one shout (vanilla has no `shout` command); untagged text goes as `s <text>` in the shout channel. | Channels stop working, or messages go out as local talk. |
| `Chat.Update` | Prefix + postfix | Takes over the window's visibility: fade, idle opacity, hiding, click-through, scrolling 3 messages per press (the prefix records the scroll position before vanilla's 1-message scroll while typing), channel placeholder. | Window shows and hides as vanilla. |
| `Terminal.UpdateChat` | Postfix | Adds the hanging-indent tags and paragraph spacing to the rebuilt output, and drops the oldest lines that don't fit the text area. | Layout back to vanilla; messages spill above the window. |
| `Terminal.AddString` | Transpiler + prefix (4-parameter overload: user, text, type, timestamp) | Replaces the shout `ToUpper` and whisper `ToLowerInvariant` calls; jumps the window to the newest line. | Warning in the log if the case calls aren't found; capitals return. |
| `Chat.AddInworldText` | Transpiler | Same case fix for the floating text over the speaker. | Same warning; capitals return over heads. |
| `Chat.GetShoutWorldTexts` | Prefix | Returns nothing when Hide shout map pings is on. | Shout pins return on the map. |

### Private members reached by reflection

Fields, properties and methods read or called through `AccessTools`, `Traverse` or `System.Reflection`. These fail
at runtime, not compile time, if renamed.

| Game code | Kind | Used for |
|---|---|---|
| `Chat.m_hideTimer` | Field (private) | Seconds since the last message or typing; drives the fade. |
| `Terminal.m_scrollHeight` | Field (private) | Scrolling without typing; reset on a new message. |
| `Terminal.m_chatBuffer` | Field (protected) | Scroll limit (buffer count − 5, as vanilla). |
| `Terminal.UpdateChat` | Method (private) | Redraws the window after scrolling or a layout setting change. |
| `Chat.InputText` | Method (protected) | Patched by name. |
| `Chat.AddInworldText` | Method (private) | Patched by name. |

### Public game code called or read

Methods, fields and components used directly. A rename or signature change breaks the build, which is easy to spot;
a change in behaviour isn't, so list the ones whose behaviour the mod depends on.

| Game code | Used for |
|---|---|
| `Chat.instance` | Refreshing the layout when a setting changes. |
| `Chat.m_input` | The input field: its text (routing), whether it's showing (typing), its placeholder (channel indicator), its rect (a strip along the bottom of the window) and `pointSize`. |
| `Terminal.m_chatWindow` | The window whose visibility, CanvasGroup alpha, size and position we set. |
| `Terminal.m_output` | The window's TextMeshPro text: indent tags, `paragraphSpacing`, font size, margin, rect, and `GetPreferredValues` to fit messages. |
| `Terminal.m_maxVisibleBufferLength` | Raised from vanilla's 20 to 100; the window height decides how many show. |
| `Terminal.TryRunCommand` | Sending an untagged message as a shout (`s <text>`). |
| `Terminal.AddString` | The grey channel-switch notice (string overload). |
| `Player.InIntro` | Hide the window during the intro. |
| `Player.InCutscene` | Hide the window in cutscenes. |
| `Hud.m_userHidden` | Hide the window with the HUD (Ctrl+F3). |
| `Menu.IsVisible` | Hide the window in the Esc menu (unless the Configuration Manager window is open). |
| `Minimap.IsOpen` | Hide the window under the big map (unless typing). |
| `Minimap.InTextInput` | No scrolling while typing a pin name. |
| `Console.IsVisible` | No scrolling while the F5 console is open. |
| `TextInput.IsVisible` | No scrolling while a sign or text popup is open. |
| `ZInput.GetButtonDown` | Reads `ScrollChatUp`/`ScrollChatDown` (and the Joy versions) while not typing. |

### Behaviour the mod assumes

Things the game does that the mod counts on without calling them, e.g. "only the owner runs
`Pickable.UpdateRespawn`". These break silently, so name the code where the behaviour lives.

| Game code | Assumption |
|---|---|
| `Chat.InputText` | Untagged input runs as `say <text>`, and `/`-prefixed input runs as a command without the slash. |
| `Terminal.InitTerminal` | The `s` command shouts `FullLine.Substring(2)`, and `say` sends local talk; `/s` and `/say` with text keep working as vanilla. |
| `Chat.Update` | Shows the window while `m_hideTimer < m_hideDelay` and resets `m_hideTimer` on focus; our postfix runs after and overrides the active state. Shows `m_input` while typing and hides it on send or close. |
| `Chat.Awake` | Sets `m_maxVisibleBufferLength = 20` before our postfix raises it. |
| `Chat.OnNewChatMessage` | Resets `m_hideTimer` on every message except pings, and replaces `<` and `>` in incoming text (so players can't inject the layout tags). |
| `Terminal.UpdateInput` | Reads the scroll buttons only while the input is focused, 1 message per press, inside `Chat.Update`; our postfix sets the position from the value before it, so the two never add up. |
| `Terminal.UpdateChat` | Rebuilds `m_output.text` as one buffer entry per line, joined with newlines (the hanging indent relies on one paragraph per message). |
| `Minimap.UpdateShoutPins` | Clears its list, then fills it from `Chat.GetShoutWorldTexts`; an empty list removes the pins. |

## Prefabs and assets

Not game code, so no script can check these. They come from the game's asset bundles: vanilla prefabs the mod
clones or reads, child objects it finds by name, materials, shader properties and layer names. Test them in game
after each update.

| Prefab or asset | Used for | Breaks if |
|---|---|---|
| Chat window (`Chat.m_chatWindow`, `Chat_box` in the main scene) | Gets a `CanvasGroup` added for alpha and click-through. Its stored size (500 x 400) and bottom gap (67) are read at login as the base for Width, Height and the new gap. It has its own Canvas, so at runtime it covers the screen; we don't move it. | Something else drives a CanvasGroup on it, or the window is restructured. |
| Chat window children `root` and `root/bkg` (an `Image`) | `root` is the visible box: anchored and pivoted to Chat_box's bottom-right corner, `anchoredPosition` (-gap, gap), sized from the settings, and given a `RectMask2D`. Laid out again on the first frame and on resolution changes. `bkg` is stretched to fill it and its gradient sprite cleared, keeping its colour. | Renamed or moved: the log line says `MISSING` and the background keeps its gradient. |
| Chat input character limit (`m_input.characterLimit`, 127 on ChatInput in the main scene) | Raised to 200. Assumes nothing else limits length: `Chat.SendText`, the routed RPC and `Chat.OnNewChatMessage` don't. | The game adds its own length check: longer messages get cut or dropped. |
| Chat input text area (`m_input.textViewport`, "Text Area") | Side offsets set to the message padding; its `RectMask2D` padding (vanilla -8, clipping beyond its edges) set to 0. | Restructured: long typed lines run to the edge again. |
| Chat output and input rects (`m_output`, `m_input`) | Set explicitly: input a strip along the bottom, its height vanilla's (35.5) scaled with font size; output fills the space above, 6 units in from the edges, margin cleared. Vanilla's stored child positions don't match what's drawn, so none are kept. | The window is rebuilt differently: check sizes in game. |
| Chat input placeholder (`m_input.placeholder`, a TextMeshPro text) | Shows `[Shout]` / `[Say]` before vanilla's placeholder text. | The placeholder stops being TextMeshPro text: the indicator silently stops. |
| BepInEx Configuration Manager (`com.bepis.bepinex.configurationmanager`, optional mod) | Its public `DisplayingWindow` property, read by reflection, keeps the chat up while its window is open. | Renamed: the chat hides behind the Esc menu again while changing settings. |
| Key bindings `ScrollChatUp`, `ScrollChatDown` (Page Up, Page Down) | Scrolling without typing. | Renamed in `ZInput`. |
