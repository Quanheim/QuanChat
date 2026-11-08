# QuanChat: design

Status: v1 agreed with Quandru, 7 Nov 2026. **Released as 1.0.0, 8 Nov 2026**, after every solo test step passed (see Test results); steps 1, 8, 15b and 21b, which need a second player, are still to check on the server. Valheim 1.0.21.

## Purpose

A chat mod for the Quanheim server with a feature set close to **BetterChat 1.6.4** (Crystal), fixing what bugs us
most about vanilla chat. No other chat mod is in our modpack, so QuanChat has the chat to itself.

## What vanilla does today (from `Chat` and `Terminal`)

- **Three message types:** normal talk (`/say`, reaches players within 15 m), shout (`/s`, reaches everyone) and
  whisper (`/w`, 4 m; despite its help text it isn't a private message to a named player).
- **The default is normal talk**, so an untagged message only reaches players in range. That's most likely why
  messages get missed: they were never delivered, not hidden.
- **Shouts are forced to capitals** and whispers to lowercase, both in the chat window (`Terminal.AddString`) and in
  the floating text over the speaker (`Chat.AddInworldText`).
- **Every shout flashes a pin on the big map** at the sender (`Minimap.UpdateShoutPins` reads
  `Chat.GetShoutWorldTexts`) for the 5 s its floating text lasts (`Chat.m_worldTextTTL`).
- **Floating text over the speaker** shows for every message type to players within 50 m or with the map open
  (`Chat.OnNewChatMessage`, `Minimap.m_nomapPingDistance`).
- **The window hides 10 s after the last message** (`Chat.m_hideDelay`, `m_hideTimer`); new messages bring it back,
  except pings.
- **Page Up / Page Down scroll** the history (`ScrollChatUp`/`ScrollChatDown`), but only while typing: the input
  handling lives in `Terminal.Update`'s focused branch. History is 300 lines, 20 visible.
- **Shouts go straight from sender to each player** (`Chat.SendText` → one routed RPC per player). The server only
  forwards routed RPCs (`ZRoutedRpc.RPC_RoutedRPC`); it needs no handler of its own.
- **Incoming text has `<` and `>` replaced with spaces**, so players can't inject formatting tags. Any formatting we
  add is applied locally.
- **Tab completion exists:** chat runs on the console's command system, which completes commands and their
  arguments (`Terminal`, `ConsoleCommand` option lists).

## v1 (agreed 7 Nov 2026)

| # | Feature | What it means |
|---|---|---|
| 1 | Shout is the default channel | Typing a message and pressing Enter sends it to everyone on the server. |
| 2 | One-off channel messages | `/say hello` sends that one message as local talk (15 m) and leaves your channel as it was. Likewise `/s hello` or `/shout hello`. |
| 3 | Sticky channel switching | `/say` on its own, then Enter, makes local talk your channel for every later untagged message. `/s` or `/shout` on its own switches back to shout. WoW style. |
| 4 | Channel indicator | The empty chat input shows your channel as placeholder text, e.g. "[Shout] …". Judged clear and readable in the test (7 Nov). |
| 5 | Starting channel | Every login starts you in your *Starting channel* setting (Shout when the config file is first created; each player can change it). Your sticky choice during play isn't remembered between logins. |
| 6 | Messages as typed | No forced capitals on shouts or lowercase on whispers, in the window or the floating text. |
| 7 | Window visibility | A new message shows the window at full opacity. After *Seconds until idle* (30 s) it fades **to the idle opacity** (36 %) and stays there until the next message, with *Always show* on (the default from 0.2.6), or **out entirely** with it off. |
| 8 | Hidden when it should be | The window hides with the HUD (Ctrl+F3), during the intro and in menus, and sits **under the big map**, not over it. |
| 9 | Scroll without typing | Page Up / Page Down scroll the window whenever it's showing, not only while typing, 3 messages per press (Quandru, 8 Nov). A new message jumps back to the bottom. |
| 10 | Click-through | The window doesn't catch mouse clicks while you're not typing in it. |
| 11 | Hide shout map pings | Shouts don't flash a pin on your map. The floating text over a nearby speaker stays. Personal, on by default. |
| 12 | Hanging indent | The sender's name starts at the margin and the message, wrapped lines included, is indented after it, so each new message visibly starts at the margin. TextMeshPro `<indent>`/`<line-indent>` tags. |
| 13 | Space between messages | A small gap, about half a line, between messages; off by default from 0.2.6. TextMeshPro `<line-height>` or `<space>`-style tags. |
| 14 | Even background | One shade of black over the whole window instead of vanilla's gradient, so it reads the same at idle opacity. Added after test step 10 (Quandru, 7 Nov). |
| 15 | Window size and position | *Width* and *Height* settings (percent of vanilla's 500 x 400), anchored bottom right. The gap below is half vanilla's, with the same gap on the right. Added 7 Nov. |
| 16 | Messages fit the window | Only as many messages as fit the window show; the oldest drop off the top instead of spilling above it. A taller window shows more (up to 100). Added 7 Nov. |
| 17 | Font size | *Font size* setting in points (vanilla 18); the input box grows with it. Added 7 Nov. |
| 18 | Longer messages | Up to 200 characters instead of vanilla's 127 (the input's own limit; nothing else checks length). The floating text over the speaker shows the whole message; to be judged. Added 7 Nov. |

## Later (agreed 7 Nov 2026)

- **Private messages, the first update:** `/w <name> <message>` reaches one player wherever they are; nobody else
  sees it, admins included. It takes over vanilla's `/w`, so the 4 m whisper channel goes. Sent as a routed RPC to
  that player's peer; the server only forwards it, and QuanChat never logs it. (It still crosses the network as plain
  text, as all Valheim chat does.)
- **Tab completion of names** for `/w`: Tab cycles through online players, narrowed by what you've typed.
- **Colour per player** (accessibility): each sender's name in its own colour, on your screen only, assigned fresh
  each session. Name only, so yellow still means shout.
- **Talk distance** as a server setting (vanilla 15 m).
- **Parked:** timestamps, `/r` to reply to the last private message, history kept between sessions, a server chat
  log (**must exclude private messages**), mentions with a sound, server announcements, a Discord bridge, colours,
  filters for join, leave and death messages.

## Settings

Per the template convention (`docs/DEVELOPMENT.md`, "Config files"). **All personal** (Quandru, 7 Nov 2026): nothing
in v1 affects other players, so nothing is synced from the server. Talk distance (later) would be the first enforced
setting.

**The server writes the config file anyway** (1.0.1, 8 Nov 2026). Gale's profile sync, checked in its source
(`profile/import/mod.rs`, `profile/export/mod.rs`, 8 Nov 2026), deletes a player's config files that aren't in the
pushed profile unless that player has "preserve extras" on. The only way to keep them is to exclude the file from the
owner's export, and Gale only lists, and only keeps an exclusion for, files that exist in the owner's profile. 1.0.0
skipped the config on a dedicated server, so there was nothing to exclude. So `quandru.QuanChat.cfg` must exist on
the server and be excluded from the master profile's export.

| Section | Setting | Default | Enforced / Personal | Description |
|---|---|---|---|---|
| `0 - General (personal)` | Starting channel | Shout | Personal | The channel you start in each time you log in. Shout reaches everyone; Say reaches players within 15 m. Switch during play with `/s` or `/say` on their own. |
| `1 - Window (personal)` | Always show | On | Personal | Off: the chat window fades out entirely once it's idle. On: it stays on screen at the idle opacity. |
| | Seconds until idle | 30 | Personal | How long the window stays at full opacity after the newest message (5–300). |
| | Idle opacity | 36 % | Personal | How visible the window stays once idle, when Always show is on (10–100 %). |
| | Click-through | On | Personal | The chat window ignores mouse clicks while you aren't typing. |
| | Width | 130 % | Personal | Width of the window, in percent of the game's own (50–300). |
| | Height | 115 % | Personal | Height of the window, in percent of the game's own (50–300). Shows as many messages as fit. |
| `2 - Messages (personal)` | Hanging indent | On | Personal | Indent each message after the sender's name, so new messages start at the margin. |
| | Space between messages | Off | Personal | A small gap between messages. |
| | Hide shout map pings | On | Personal | Shouts don't flash a pin on your map. |
| | Font size | 18 | Personal | Size of the chat text in points (10–40). The input box grows with it. |

## How it would work (outline)

- **Client-side, all of v1:** channels, formatting, the window, scrolling and click-through patch `Chat` and
  `Terminal` on each player's PC. The server runs no QuanChat code in v1.
- **Channels:** our own state (current channel) decides the type for untagged input; `/say` and `/s` are replaced
  so that with no text they switch channel instead of sending an empty message.
- **No forced case:** replace the case changes in `Terminal.AddString(title, text, type)` and
  `Chat.AddInworldText`, keeping the colours.
- **Window:** take over `m_hideTimer`'s effect in `Chat.Update` (the window stays active) and drive a `CanvasGroup`
  alpha instead; hide on HUD-hidden, intro and menus.
- **Under the map:** to find out in the POC how the chat and large-map canvases are ordered; if they can't be
  reordered, hide the window while the large map is open (same result, since the map is full-screen), except while
  typing.
- **Still required on every client and the server**, version-checked like the rest of the modpack, even though v1
  needs nothing from the server: it keeps everyone on the same version, and private messages and talk distance will
  need it.

## Test script (POC)

Release zip imported into a Gale test profile (not Debug-deployed), local dedicated server or the test server, plus
a second player for the multi-player checks.

| # | Do | Expect |
|---|---|---|
| 1 | Log in, type `hello`, Enter | Placeholder showed "[Shout] …" before typing. Second player, anywhere, sees `hello` in normal case and yellow. |
| 2 | **Judge the channel indicator** | Is the placeholder readable and clear enough? Decide whether it needs proper UI. |
| 3 | `/say hi`, then an untagged `back` | `hi` reaches only players within 15 m; `back` is a shout again. |
| 4 | `/say` alone, Enter; then `one`, `two` | Placeholder shows "[Say] …"; both messages are local talk. |
| 5 | `/s` alone, Enter; then `three` | Placeholder back to "[Shout] …"; `three` reaches everyone. |
| 6 | Relog after step 4 | You start in Shout again. |
| 7 | Shout with Caps Lock off and on, mixed case | Shown exactly as typed, in the window and over your head. |
| 8 | Second player shouts while your big map is open | No pin flashes on your map. Turn *Hide shout map pings* off: the pin flashes. |
| 9 | Wait after a message (Always show off) | Full opacity for 30 s, then the window fades out entirely. |
| 10 | Always show on, idle opacity 50 % | After 30 s it fades to half and stays. A new message brings it back to full. Change both settings: they apply. |
| 11 | Ctrl+F3; Esc menu; open the big map | The chat window hides with the HUD, in the menu, and doesn't show over the map. |
| 12 | Page Up / Page Down without opening chat | The window scrolls back and forward; a new message jumps to the bottom. |
| 13 | Click where the window overlaps something (e.g. with the inventory open) | The click goes through. |
| 14 | Several messages from two players, some long enough to wrap | Each starts at the margin; wrapped lines are indented; a half-line gap between messages. Turn each setting off: back to vanilla layout. |
| 15 | Change your *Starting channel* to Say, relog | You start in Say. The second player still starts in Shout. |
| 16 | (0.2.0) Always show on, wait until idle | The background is one even shade, no gradient. The gap below the window is half what it was, with the same gap on the right. |
| 17 | (0.2.0) Width and Height to 150, then 60 | The window grows left and up, then shrinks, staying anchored bottom right; the input box spans the width. |
| 18 | (0.2.0) Fill the chat with messages | None spill above the window; the oldest drop off the top. Page Up still scrolls back. |
| 19 | (0.2.0) Font size to 26, then 12 | Text and input box grow, then shrink; messages still fit. |
| 20 | (0.2.1) Check the BepInEx log | One line: "Chat window laid out on a ... screen: box 500 x 400, 34 in from the bottom-right corner, ... root found, window <screen size in canvas units>, background made even." |
| 21 | (0.2.4) Type a message past 127 characters | It keeps going to 200 and arrives whole for the second player. Judge the floating text over your head at that length. |

## Test results

- **7 Nov 2026 (Quandru):** step 2, the placeholder is clear and readable, so it stays as the channel indicator.
  Step 3 works. Screenshots also showed the hanging indent on wrapped lines, the gap between messages, and the
  join shout "I have arrived!" in normal case.
- **7 Nov 2026 (Quandru):** step 4 works: "[Say] …" placeholder, the grey channel notice, and `one`/`two` as
  local talk (white, floating text over the speaker).
- **7 Nov 2026 (Quandru):** step 5 works: back to "[Shout] …", and `three` is a shout. Its floating text over the
  speaker is in normal case too (part of step 7).
- **7 Nov 2026 (Quandru):** step 6 works: switched to Say and sent `test before logout`, relogged, started in
  Shout.
- **7 Nov 2026 (Quandru):** step 7 works: `small letters`, `Mixed Case Throughout` and `I AM FEELING SHOUTY`
  shown as typed in the window and over the speaker. Step 9 works: the window fades out completely.
- **7 Nov 2026 (Quandru):** step 10 works: with Always show on the window comes in already showing, fades to idle
  after 30 s, comes back on a message and fades again; turning Space between messages off applies at once. But the
  gradient background doesn't suit Always show, and the window needs sizing, an even gap, messages kept inside it
  and a font size: features 14 to 17, built in 0.2.0.
- **7 Nov 2026 (Quandru), 0.2.0:** the chat filled the whole width of the screen at the bottom left. Chat_box has
  its own Canvas, which Unity stretches to the screen, so resizing it did nothing and its children filled the
  screen. 0.2.1 places the inner `root` box instead, from the screen's bottom-right corner.
- **7 Nov 2026 (Quandru), 0.2.1:** the box sat half off the bottom-right of the screen. The log said "screen canvas
  MISSING": `GetComponentInParent` skips the hidden window, so root kept its stored position. 0.2.2 anchors root
  to Chat_box's bottom-right corner instead (Chat_box covers the screen).
- **7 Nov 2026 (Quandru), 0.2.2:** right size (500 x 400) and place on login, even background (step 16 passes),
  input box along the bottom. Seen in the screenshot: a long line in the input runs to the box's left edge with
  no padding. Opening the mod settings window (from the Esc menu) hid the chat, so changes couldn't be seen.
  0.2.3 keeps the chat up while the Configuration Manager window is open, and pads the typed text.
- **7 Nov 2026 (Quandru), 0.2.2:** Height works: the box grows upwards from the corner. Width not yet tried (it
  sorts last in its section, below Seconds until idle).
- **7 Nov 2026 (Quandru):** step 18 works: with the window full, the oldest messages drop off the top of the box and
  nothing spills above it.
- **7 Nov 2026 (Quandru):** Width works too: the box grows to the left from the corner and the text rewraps to fit.
  Step 17 passed.
- **7 Nov 2026 (Quandru):** hanging indent works on a long message (wrapped line indented under the first). The
  message stopped at 127 characters: that's the chat input's `characterLimit` in the main scene (ChatInput, 127),
  client-side only; nothing in `Chat` or the routed RPC limits length.
- **7 Nov 2026 (Quandru):** step 11 works: the chat hides in the Esc menu, with Ctrl+F3 and under the big map.
- **8 Nov 2026 (Quandru):** step 12 works: Page Up / Page Down scroll without typing. Asked for 3 messages per
  press instead of 1: 0.2.5, typing or not.
- **8 Nov 2026 (Quandru):** step 13 works: with the inventory open, the craft button's hover shows through the chat.
  Step 14 done: turning Hanging indent off gives the vanilla layout.
- **8 Nov 2026 (Quandru):** step 19 works at the extremes, Font size 40 and 10: text rewraps, the hanging indent
  holds, and messages still fit the box.
- **8 Nov 2026 (Quandru), 0.2.5:** the chat stays visible while the Configuration Manager (F1) window is open.
- **8 Nov 2026 (Quandru), 0.2.5:** Page Up / Page Down move 3 messages per press.
- **8 Nov 2026 (Quandru):** step 15, first half: Starting channel Say, relog, starts in Say. Defaults set from what
  Quandru settled on in testing: Always show on, Idle opacity 36, Width 130, Height 115, Space between messages
  off (others unchanged). 0.2.6.
- **8 Nov 2026 (Quandru):** `/shout` didn't switch to shout (vanilla has only `/s`). 0.2.7 adds `/shout`, alone
  and with text.
- **8 Nov 2026 (Quandru):** step 21, own side: the message stops at exactly 200 characters, and the floating text
  over the speaker runs to five lines. Still to check: the second player receives it whole.
- **8 Nov 2026 (Quandru), 0.2.7:** step 22 works: `/shout` alone switches to Shout; `/shout hello` from Say sends
  one shout and leaves the channel in Say. Left for a second player: steps 1, 8, 15b, 21b.
- **8 Nov 2026 (Quandru):** released 1.0.0 (0.2.7's code, plus a real icon) with the second-player steps
  still to check once it's on the server.
