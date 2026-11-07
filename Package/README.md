# QuanChat

Chat you don't miss. In vanilla Valheim an ordinary message only reaches players within 15 m, shouts are forced
into capitals, and the chat window vanishes ten seconds after the last message. QuanChat fixes all three.

**Install it on the server and on every client.** It's version-checked: players with a different version can't
connect. On a dedicated server it does nothing except keep everyone's version in step.

## What it does

- **Shout is the default.** Type a message and press Enter: everyone on the server sees it.
- **Sticky channels.** `/say` on its own switches you to local talk (players within 15 m) for every message after
  it; `/s` on its own switches back to shout. `/say hello` or `/s hello` sends just that one message and leaves your
  channel alone. The chat input shows which channel you're in, e.g. `[Shout]`. You start each login in your
  *Starting channel* (Shout unless you change it).
- **Messages appear as typed.** No forced capitals on shouts, no forced lowercase on whispers.
- **A chat window you can read.** A new message shows it at full opacity for 30 seconds, then it fades out; or, with
  *Always show* on, it fades to half opacity and stays on screen. It hides with the HUD (Ctrl+F3), in the menu and
  under the big map, and lets mouse clicks through while you aren't typing. One even background, sized to taste
  with *Width* and *Height*, and messages never spill above it.
- **Scroll without typing.** Page Up and Page Down scroll back through chat any time the window can show. A new
  message jumps back to the newest line.
- **Longer messages.** Up to 200 characters instead of the game's 127.
- **Your font size.** Set the chat text size you like.
- **Easier to tell messages apart.** Each message starts at the margin with its wrapped lines indented, and there's
  a small gap between messages.
- **No shout map pings.** Shouts don't flash a pin on your map.

## Settings

All personal: each player sets their own, and nothing is synced from the server.

| Section | Setting | Default | What it does |
|---|---|---|---|
| 0 - General (personal) | Starting channel | Shout | The channel you start in each time you log in. |
| 1 - Window (personal) | Always show | Off | Off: the window fades out once idle. On: it stays at the idle opacity. |
| | Seconds until idle | 30 | How long the window stays at full opacity after the newest message. |
| | Idle opacity | 50 | How visible the window stays once idle, in percent, when Always show is on. |
| | Click-through | On | The window ignores mouse clicks while you aren't typing. |
| | Width | 100 | Width of the window, in percent of the game's own. |
| | Height | 100 | Height of the window, in percent of the game's own; it shows as many messages as fit. |
| 2 - Messages (personal) | Hanging indent | On | Indent wrapped lines so each message starts at the margin. |
| | Space between messages | On | A small gap between messages. |
| | Hide shout map pings | On | Shouts don't flash a pin on your map. |
| | Font size | 18 | Size of the chat text, in points. The input box grows with it. |

## Source and issues

<https://github.com/Quanheim/QuanChat>. Made for the Quanheim server, with Claude.
